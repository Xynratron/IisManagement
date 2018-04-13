using System.Collections.Generic;

namespace IisManagement.Shared
{
    public class IisSite
    {
        public string Group { get; set; }
        public string SiteName { get; set; }
        public List<string> Domains { get; set; }
        public bool AddPictures { get; set; }
        public bool LocalPictures { get; set; }
        public string AppPoolName { get; set; }
        public string AppPoolUser { get; set; }
        public string AppPoolPassword { get; set; }
        public string Version { get; set; }
    }
}