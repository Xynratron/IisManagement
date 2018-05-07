using System;
using IisManagement.Server.FileManagement;
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
                var site = GetWebsite();
                if (site == null)
                {
                    Logger.Info($"Could not find Site {message.SiteName}");
                    return new DefaultResult { Success = false };
                }
                var sitePath = site.Applications["/"].VirtualDirectories["/"].PhysicalPath;
                Logger.Info($"Found Site Content at {sitePath}");

                var backupPath = GetDeploymentPath();

                Logger.Info($"Creating Backup at Deployment Location: {backupPath}");


                if (!ImpersonatedFiles.Exists(backupPath))
                {
                    Logger.Info($"Could not Find New Version of Site");

                    ImpersonatedFiles.CreateDirectory(backupPath);

                    Logger.Info($"Copy Old Site to Deployment as initial Version");
                }

                ImpersonatedFiles.CopyFilesRecursively(sitePath, backupPath);

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