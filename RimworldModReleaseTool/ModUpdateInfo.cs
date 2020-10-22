using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace RimworldModReleaseTool
{
    public class ModUpdateInfo
    {
        
        public ModUpdateInfo(string modRootFolder)
        {
            Path = modRootFolder;

            var steamPublishIDPath = Path + @"\About\PublishedFileId.txt";
            if (File.Exists(steamPublishIDPath))
            {
                var steamPublishID = File.ReadLines(steamPublishIDPath).First();
                SteamURL = @"https://steamcommunity.com/sharedfiles/filedetails/?id=" + steamPublishID;
            }

            ///// Get the name
            var modName = ParseAboutXMLFor("name", Path);
            var modAuthor = ParseAboutXMLFor("author", Path);

            Name = modName; //path.Substring(path.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            Author = modAuthor;

            var changelogPath = Path + @"\About\Changelog.txt";
            var manifestPath = Path + @"\About\Manifest.xml";

            var changelogFile = new FileInfo(changelogPath);
            var manifestFile = new FileInfo(manifestPath);
            if (changelogFile.Exists && manifestFile.Exists)
            {
                string currentVersion = null;
                foreach (var line in File.ReadAllLines(manifestFile.FullName))
                {
                    if (!line.Contains("<version>"))
                    {
                        continue;
                    }
                    currentVersion = line.Replace("<version>", "|").Split('|')[1].Split('<')[0];
                }
                if (!string.IsNullOrEmpty(currentVersion))
                {
                    bool isExtracting = false;
                    string changelogMessage = null;
                    Regex versionRegex = new Regex(@"\d+(?:\.\d+){1,3}");
                    foreach (var line in File.ReadAllLines(changelogFile.FullName))
                    {
                        if (line.StartsWith(currentVersion))
                        {
                            isExtracting = true;
                            changelogMessage += line;
                            continue;
                        }
                        Match match = versionRegex.Match(line);
                        if (isExtracting)
                        {
                            if (match.Success)
                                break;
                            changelogMessage += line;
                            continue;
                        }
                    }
                    LatestChangeNote = changelogMessage;
                }
            }
        }


        public string Path { get; }

        public string Name { get; }
                
        public string Author { get; }

        public string SteamURL { get; }

        public string LatestChangeNote { get; private set; }

        private static string ParseAboutXMLFor(string element, string newPath)
        {
            var text = newPath + @"\About\About.xml";
            var xml = new XmlDocument();
            xml.Load(text);
            return XElement.Parse(xml.InnerXml).Element(element)?.Value ?? "NULL";
        }
    }
}