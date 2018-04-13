using System.Collections.Generic;

namespace IisManagement.Shared
{
    public class DeleteWebsiteRequest
    {
        public bool KeepLocalFiles { get; set; }
        public IisSite SiteInformation { get; set; }
    }
}