using System;
using System.IO;
using System.Linq;
using IisManagement.Shared;
using Microsoft.Web.Administration;
using NLog;

namespace IisManagement.Server.Worker
{
    public class SiteManagement
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

        protected void CopyFilesRecursively(string sourceDirectory, string targetDirectory)
        {
            Logger.Info($"Copy Files from {sourceDirectory} to {targetDirectory}");

            if (Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            CopyFilesRecursively(new DirectoryInfo(sourceDirectory), new DirectoryInfo(targetDirectory));

            Logger.Info($"Finished Copying Files from {sourceDirectory} to {targetDirectory}");
        }
        private void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (var file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
    }
}