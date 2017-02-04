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

        public Form1()
        {
            InitializeComponent();

            foreach (var path in Directory.GetFiles(@"C:\Program Files (x86)\Steam\steamapps\", "*.acf", SearchOption.AllDirectories))
                ReadConfigObject(File.OpenText(path));
            foreach (var path in Directory.GetFiles(@"D:\SteamLibrary\steamapps", "*.acf", SearchOption.AllDirectories))
                ReadConfigObject(File.OpenText(path));
            foreach (var path in Directory.GetFiles(@"F:\SteamLibrary\steamapps", "*.acf", SearchOption.AllDirectories))
                ReadConfigObject(File.OpenText(path));
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
                    var value = match.Groups[2].Value;

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
    }
}
