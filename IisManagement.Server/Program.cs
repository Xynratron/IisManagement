using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IisManagement.Server.Settings;
using IisManagement.Server.Worker;
using Topshelf;
using Microsoft.Extensions.Configuration;
using Microsoft.Web.Administration;

namespace IisManagement.Server
{
    public class Program
    {
        
        private static void Main()
        {
            InitializeSettings();

            var rc = StartService();                                                             

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  
            Environment.ExitCode = exitCode;
        }

        private static TopshelfExitCode StartService()
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
            return rc;
        }

        private static void InitializeSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();
            config.Bind(new ServerSettings());
        }
    }
}
