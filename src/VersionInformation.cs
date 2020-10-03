using System.Collections.Generic;

namespace Launcher.src
{
    public class VersionInformation
    {
        public class Manifest
        {
            public int TotalFiles { get; set; }
            public int TotalFolders { get; set; }
        }

        public class File
        {
            public string name { get; set; }
            public string checksum { get; set; }
        }

        public Manifest manifest { get; set; }
        public List<string> Folders { get; set; }
        public List<File> Files { get; set; }
    }
}
