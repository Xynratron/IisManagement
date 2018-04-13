using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IisManagement.Shared;

namespace IisManagement.Server.Worker
{
    public class CreateWebsite : IWorker<CreateWebsiteRequest, DefaultResult>
    {
        public DefaultResult ReceiveAndSendMessage(CreateWebsiteRequest message)
        {

            return new DefaultResult();
        }
    }
}
