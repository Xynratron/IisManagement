using System;
using IisManagement.Server.FileManagement;
using IisManagement.Shared;
using NLog;

namespace IisManagement.Server.Worker
{
    public class ReadServerContentsWorker : SiteManagement, IWorker<ReadServerContentsRequest, ReadServerContentsResponse>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ReadServerContentsResponse ReceiveAndSendMessage(ReadServerContentsRequest message)
        {
            try
            {
                Logger.Info("Starting");
                
                Logger.Info("Finished");
                return new ReadServerContentsResponse { Success = true };
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return new ReadServerContentsResponse { Success = false };
        }
    }
}