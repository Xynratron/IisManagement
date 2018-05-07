using System;
using System.Collections.Generic;
using System.Text;

namespace IisManagement.Shared
{
    public class RenameWebsiteRequest
    {
        public string CurrentName { get; set; }
        public IisSite SiteInformation { get; set; }
    }
}
