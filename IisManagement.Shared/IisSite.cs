using System;
using System.Collections.Generic;

namespace IisManagement.Shared
{
    public class IisSite
    {
        public string Name
        {
            get
            {
                var result = SiteName;
                if (SiteName.StartsWith(Group, StringComparison.OrdinalIgnoreCase))
                {
                    result = SiteName.Remove(0, Group.Length).TrimStart(new[] {' ', '-', '_'});
                }
                return result;
            }
        }

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