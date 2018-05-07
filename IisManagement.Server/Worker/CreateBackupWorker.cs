using System;
using System.IO;
using System.Linq;
using IisAdmin;
using IisManagement.Shared;
using NLog;

namespace IisManagement.Server.Worker
{
    public class CreateBackupWorker : SiteManagement, IWorker<CreateBackupRequest, DefaultResult>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public DefaultResult ReceiveAndSendMessage(CreateBackupRequest message)
        {
            try
            {
                Logger.Info("Starting Backup");
                
                Logger.Info("Finished Backup");
                return new DefaultResult { Success = true };
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return new DefaultResult { Success = false };
        }
    }
}