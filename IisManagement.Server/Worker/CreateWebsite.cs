using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IisAdmin;
using IisManagement.Shared;
using NLog;

namespace IisManagement.Server.Worker
{
    public class CreateWebsite : IWorker<CreateWebsiteRequest, DefaultResult>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IisSite _currentSite;
        public DefaultResult ReceiveAndSendMessage(CreateWebsiteRequest message)
        {
            try
            {
                Logger.Info("Starting CreateWebsite");
                _currentSite = message.SiteInformation;
                ManipulateHostsFile();
                ChangeWebsite();
                CopyContents();
                Logger.Info("Finished CreateWebsite");
                return new DefaultResult{Success = true};
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
            ServerHostsFile.CheckHostsFile(_currentSite);
            ServerHostsFile.RemoveDuplicateEmptyHostEntries();
        }
    }
}
