using System;
using System.Collections.Generic;
using System.Text;

namespace IisManagement.Shared
{
    public class CreateWebsiteRequest
    {
        public IisSite SiteInformation { get; set; }
    }

    public class ReadServerContents
    {
        public List<IisSite> SiteInformations { get; set; }
    }

    public class IisSite
    {
        public string Group { get; set; }
        public string SiteName { get; set; }
        public List<string> Domains { get; set; }
        public bool AddPictures { get; set; }
        public bool LocalPictures { get; set; }
        public string AppPoolName { get; set; }
    }
}
