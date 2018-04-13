using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using IisManagement.Shared;

namespace IisManagement.Client.Console
{
    class Program
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        
        static void Main(string[] args)
        {
            try
            {
                var message = new CreateWebsiteRequest();
                var result = Communication.SendMessageToServer< CreateWebsiteResult, CreateWebsiteRequest>(message);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
    }
}
