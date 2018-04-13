using System;
using IisAdmin;
using IisManagement.Shared;
using NLog;

namespace IisManagement.Server.Worker
{
    public class DeleteWebsite : IWorker<DeleteWebsiteRequest, DefaultResult>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IisSite _currentSite;
        public DefaultResult ReceiveAndSendMessage(DeleteWebsiteRequest message)
        {
            try
            {
                Logger.Info("Starting CreateWebsite");
                _currentSite = message.SiteInformation;
                ManipulateHostsFile();
                ChangeWebsite();
                CopyContents();
                Logger.Info("Finished CreateWebsite");
                return new DefaultResult { Success = true };
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return new DefaultResult { Success = false };
        }

        private void CopyContents()
        {

        }

        private void ChangeWebsite()
        {

        }

        private void ManipulateHostsFile()
        {
            ServerHostsFile.RemoveSite(_currentSite);
            ServerHostsFile.RemoveDuplicateEmptyHostEntries();
        }
    }
}