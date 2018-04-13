using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IisAdmin;
using IisManagement.Shared;
using NLog;

namespace IisManagement.Server.Worker
{
    public class CreateWebsite : SiteManagement, IWorker<CreateWebsiteRequest, DefaultResult>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public DefaultResult ReceiveAndSendMessage(CreateWebsiteRequest message)
        {
            try
            {
                Logger.Info("Starting CreateWebsite");
                CurrentSite = message.SiteInformation;
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
            EnsureSiteDirecory();
            CreateOrChangeWebsite();
            CreateOrChangeAppPool();
            CreateOrChangeVirtualPicturesDirectory();
        }

        private void CreateOrChangeVirtualPicturesDirectory()
        {

        }

        private void CreateOrChangeAppPool()
        {

        }

        private void CreateOrChangeWebsite()
        {

        }

        private void EnsureSiteDirecory()
        {
            var sitepath = GetSitePath();
            if (!Directory.Exists(sitepath))
                Directory.CreateDirectory(sitepath);
        }

        
        private void ManipulateHostsFile()
        {
            ServerHostsFile.AddSite(CurrentSite);
            ServerHostsFile.RemoveDuplicateEmptyHostEntries();
        }
    }
}
