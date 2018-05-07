using System;
using System.IO;
using System.Linq;
using IisAdmin;
using IisManagement.Shared;
using Microsoft.Web.Administration;
using NLog;

namespace IisManagement.Server.Worker
{
    public class RenameWebsite : SiteManagement, IWorker<RenameWebsiteRequest, DefaultResult>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public DefaultResult ReceiveAndSendMessage(RenameWebsiteRequest message)
        {
            try
            {
                Logger.Info("Starting RenameWebsite");
                CurrentSite = message.SiteInformation;
                _currentName = message.CurrentName;
                ChangeWebsite();
                Logger.Info("Finished RenameWebsite");
                return new DefaultResult { Success = true };
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return new DefaultResult { Success = false };
        }

        private string _currentName;
        private void ChangeWebsite()
        {
            Logger.Info("Searching for Site");
            var site = ServerManager.Sites[_currentName];
            site.Name = SiteName();
            Logger.Info("Commiting Changes");
            ServerManager.CommitChanges();
        }
    }
}