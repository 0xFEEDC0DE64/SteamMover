using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamMover
{
    public partial class Form1 : Form
    {
        private static readonly Regex regexObject = new Regex("^(?: |\t)*\"([^\"]*)\"$");
        private static readonly Regex regexObjectBegin = new Regex("^(?: |\t)*\\{$");
        private static readonly Regex regexObjectEnd = new Regex("^(?: |\t)*\\}$");
        private static readonly Regex regexVariable = new Regex("^(?: |\t)*\"([^\"]*)\"(?: |\t)*\"([^\"]*)\"$");

        private const string defaultDir = @"C:\Program Files (x86)\Steam\steamapps";

        public Form1()
        {
            InitializeComponent();

            objectListView1.AlwaysGroupByColumn = columnLibrary;

            Refresh();
        }

        void Refresh()
        {
            var libraries = new List<string> { defaultDir };
            var libraryConf = (Dictionary<string, object>)ReadConfigObject(File.OpenText(Path.Combine(defaultDir, "libraryfolders.vdf")))["LibraryFolders"];
            for (var i = 1; libraryConf.ContainsKey(i.ToString()); i++)
                libraries.Add(Path.Combine((string)libraryConf[i.ToString()], "steamapps"));

            var games = new List<Game>();
            foreach (var library in libraries)
                foreach (var appmanifestPath in Directory.GetFiles(library, "appmanifest_*.acf", SearchOption.TopDirectoryOnly))
                {
                    var appmanifestConf = (Dictionary<string, object>)ReadConfigObject(File.OpenText(appmanifestPath))["AppState"];

                    games.Add(new Game
                    {
                        id = Convert.ToInt32((string)appmanifestConf["appid"]),
                        name = (string)appmanifestConf["name"],
                        library = library,
                        directory = (string)appmanifestConf["installdir"],
                    });
                }

            objectListView1.SetObjects(games.OrderBy(o => o.name));
        }

        private Dictionary<string, object> ReadConfigObject(StreamReader streamReader, bool nestedObject = false)
        {
            var dictionary = new Dictionary<string, object>();

            while (true)
            {
                var line = streamReader.ReadLine();

                if (line == null)
                {
                    if (nestedObject)
                        throw new Exception("Unexpected end of file");
                    else
                        break;
                }

                Match match;
                if ((match = regexObject.Match(line)).Success)
                {
                    var key = match.Groups[1].Value;

                    line = streamReader.ReadLine();
                    if (!regexObjectBegin.IsMatch(line))
                        throw new Exception("Object doesnt start");

                    var value = ReadConfigObject(streamReader, true);

                    dictionary.Add(key, value);
                }
                else if ((match = regexVariable.Match(line)).Success)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value.Replace("\\\\", "\\");

                    dictionary.Add(key, value);
                }
                else if (regexObjectEnd.IsMatch(line))
                {
                    if (nestedObject)
                        break;
                    else
                        throw new Exception("Unexpected object closing");
                }
                else
                    throw new Exception("Unexpected line");
            }

            return dictionary;
        }

        private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonMove.Enabled = true;
            buttonDelete.Enabled = true;
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            Refresh();
        }

        private void buttonMove_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
