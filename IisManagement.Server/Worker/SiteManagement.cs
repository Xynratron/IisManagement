using System;
using System.IO;
using System.Linq;
using IisManagement.Shared;
using Microsoft.Web.Administration;

namespace IisManagement.Server.Worker
{
    public class SiteManagement
    {
        protected ServerManager ServerManager;
        protected IisSite CurrentSite;

        public SiteManagement()
        {
            ServerManager = new ServerManager();
        }
        protected string GetSitePath()
        {
            return Path.Combine(ServerSettings.BasePath, CurrentSite.Group, $"{CurrentSite.Name} - {CurrentSite.Version}");
        }

        protected string GetDeploymentPath()
        {
            return Path.Combine(ServerSettings.Deployment.Location, CurrentSite.Group, CurrentSite.Name, CurrentSite.Version);
        }

        protected string SiteName()
        {
            return $"{CurrentSite.Group} {CurrentSite.Name}";
        }
        protected Site GetWebsite()
        {
            return ServerManager.Sites.FirstOrDefault(o => string.Equals(o.Name, SiteName(), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}