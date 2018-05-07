using System;
using System.Linq;
using IisManagement.Server.FileManagement;
using IisManagement.Shared;
using NLog;

namespace IisManagement.Server.Worker
{
    public class DeleteWebsiteWorker : SiteManagement, IWorker<DeleteWebsiteRequest, DefaultResult>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DefaultResult ReceiveAndSendMessage(DeleteWebsiteRequest message)
        {
            try
            {
                Logger.Info("Starting DeletingWebsite");
                CurrentSite = message.SiteInformation;
                Logger.Info("Manipulating Hosts File");
                ManipulateHostsFile();
                Logger.Info("Manipulating IIS");
                ChangeWebsite();
                Logger.Info("Deleting local Files");
                RemoveSiteDirectory();
                Logger.Info("Finished CreateWebsite");
                return new DefaultResult { Success = true };
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return new DefaultResult { Success = false };
        }


        private void ChangeWebsite()
        {
            Logger.Info("Removing Site From Iis");
            RemoveSiteFromIis();
            Logger.Info("Removing Empty Application Pools");
            RemoveEmptyApplicationPools();
            Logger.Info("Commiting Changes");
            ServerManager.CommitChanges();
        }

        private void RemoveEmptyApplicationPools()
        {
            var allAppPools = ServerManager.ApplicationPools.Select(o => o.Name).ToList();
            var usedAppPools = ServerManager.Sites.Select(o => o.Applications[0].ApplicationPoolName);
            var unusedAppPools = allAppPools.Except(usedAppPools).ToList();
            foreach (var appPoolName in unusedAppPools)
            {
                ServerManager.ApplicationPools.Remove(ServerManager.ApplicationPools[appPoolName]);
            }
        }

        private string _physicalSitePath;
        private void RemoveSiteFromIis()
        {
            var site = GetWebsite();
            if (site != null)
            {
                _physicalSitePath = site.Applications["/"].VirtualDirectories["/"].PhysicalPath;
                ServerManager.Sites.Remove(site);
            }
        }

        private void RemoveSiteDirectory()
        {
            if (ImpersonatedFiles.Exists(_physicalSitePath))
                ImpersonatedFiles.Delete(_physicalSitePath, true);
        }

        private void ManipulateHostsFile()
        {
            ServerHostsFile.RemoveSite(CurrentSite);
            ServerHostsFile.RemoveDuplicateEmptyHostEntries();
        }
    }
}