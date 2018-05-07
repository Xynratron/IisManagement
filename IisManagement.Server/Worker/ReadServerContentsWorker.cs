using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IisManagement.Server.FileManagement;
using IisManagement.Shared;
using Microsoft.Web.Administration;
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
                var result = new ReadServerContentsResponse();
                result.SiteInformations = new List<IisSite>();

                foreach (var site in ServerManager.Sites)
                {
                    Logger.Info($"Reading Site {site.Name}");
                    result.SiteInformations.Add(ReadSiteInformation(site));
                }

                Logger.Info("Finished");
                result.Success = true;
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return new ReadServerContentsResponse { Success = false };
        }

        private IisSite ReadSiteInformation(Site site)
        {
            var result = new IisSite();
            
            var sitePath = site.Applications["/"].VirtualDirectories["/"].PhysicalPath;
            result.Group = FindGroupAtPath(sitePath);
            result.SiteName = site.Name;
            result.Version = FindVersionAtPath(sitePath);

            result.Domains = FindDomainsInBindings(site);
            result.AddPictures = HasPicturesVirtDir(site);
            result.LocalPictures = HasLocalPicDir(site);

            return result;
        }

        private bool HasLocalPicDir(Site site)
        {
            var app = site.Applications[0];
            var virt = app.VirtualDirectories.FirstOrDefault(o => o.Path.Equals("/Pictures", StringComparison.OrdinalIgnoreCase));
            if (virt == null)
                return false;
            return !virt.PhysicalPath.StartsWith("\\");
        }

        private bool HasPicturesVirtDir(Site site)
        {
            var app = site.Applications[0];
            var virt = app.VirtualDirectories.FirstOrDefault(o => o.Path.Equals("/Pictures", StringComparison.OrdinalIgnoreCase));
            return virt != null;
        }

        private List<string> FindDomainsInBindings(Site site)
        {
            return site.Bindings.Select(o => o.Host).ToList();
        }

        private string FindVersionAtPath(string sitePath)
        {
            var matches = Regex.Match(sitePath, @"(\d+.)+(\d+)+", RegexOptions.Compiled);
            return matches.Success ? matches.Value : "";
        }

        private string FindGroupAtPath(string sitePath)
        {
            var result = "other";
            if (string.IsNullOrWhiteSpace(sitePath))
                return result;

            var path = Path.GetDirectoryName(sitePath);
            if (!string.IsNullOrWhiteSpace(path))
            {
                result = Path.GetFileName(path);
            }
            return result;
        }
    }
}