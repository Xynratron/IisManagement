using System.Collections.Generic;

namespace IisManagement.Shared
{
    public class ReadServerContentsResponse
    {
        public bool Success { get; set; }
        public List<IisSite> SiteInformations { get; set; }
    }
}