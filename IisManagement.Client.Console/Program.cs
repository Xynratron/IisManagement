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
                //var message = new CreateWebsiteRequest
                //{
                //    SiteInformation = new IisSite
                //    {
                //        Group = "MX2",
                //        SiteName = "Borbet-B2B", 
                //        Domains = new List<string>(new [] {"borbet.mx-live.com", "www.borbet.mx-live.com" , "shop.borbet.com" }),
                //        Version = "1.0.141",
                //        AddPictures = true
                //    }
                //};
                //var result = Communication.SendMessageToServer<DefaultResult, CreateWebsiteRequest>(message);

                var result = Communication.SendMessageToServer<DefaultResult, DeleteWebsiteRequest>(new DeleteWebsiteRequest
                {
                    SiteInformation = new IisSite
                    {
                        Group = "MX2",
                        SiteName = "Borbet-B2B",
                        Domains = new List<string>(new[] { "borbet.mx-live.com", "www.borbet.mx-live.com", "shop.borbet.com" }),
                    }
                });
                
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
    }
}
