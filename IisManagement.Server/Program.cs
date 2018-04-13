using System;
using System.Collections.Generic;
using System.Linq;
using IisManagement.Server.Worker;
using Topshelf;

namespace IisManagement.Server
{
    public class Program
    {
        
        private static void Main()
        {
            var rc = HostFactory.Run(x =>                                   
            {
                x.Service<NettyServer>(s =>                                   
                {
                    s.ConstructUsing(name => new NettyServer());                
                    s.WhenStarted(tc => tc.Start());                       
                    s.WhenStopped(tc => tc.Stop());                       
                });
                x.RunAsNetworkService();                                       

                x.SetDescription("IisManagement Server");                  
                x.SetDisplayName("IisManagement Server");                                  
                x.SetServiceName("IisManagement Server");                                  
            });                                                             

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  
            Environment.ExitCode = exitCode;
        }
    }
}
