using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IisAdmin;
using IisManagement.Shared;
using Microsoft.Web.Administration;
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
                ServerManager.CommitChanges();
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
            if (_previousSitePath != GetSitePath())
            {
                //??
                //Copy from Version is better
            }
        }

        private void ChangeWebsite()
        {
            EnsureSiteDirecory();
            CreateOrChangeWebsite();
            CreateOrChangeAppPool();
            CreateOrChangeVirtualPicturesDirectory();
            AddOrChangeBindings();
            RemoveOldDirectory();
        }

        private void AddOrChangeBindings()
        {
            //Add Bindings
            foreach (var domain in CurrentSite.Domains.Select(o => o.ToLowerInvariant()))
            {
                if (!_site.Bindings.Any(o => string.Equals(o.Host, domain, StringComparison.InvariantCultureIgnoreCase)))
                    _site.Bindings.Add("*:80:" + domain, "http");
            }
            var bindingRemoverList = new List<Binding>();
            //Remove obsolete Bindings
            foreach (var binding in _site.Bindings)
            {
                if (!CurrentSite.Domains.Any(o => o.Equals(binding.Host, StringComparison.InvariantCultureIgnoreCase)))
                {
                    bindingRemoverList.Add(binding);
                }
            }
            foreach (var binding in bindingRemoverList)
            {
                _site.Bindings.Remove(binding);
            }
        }

        private void RemoveOldDirectory()
        {
            if (string.IsNullOrWhiteSpace(_previousSitePath))
                return;
            if (!Directory.Exists(_previousSitePath))
                Directory.CreateDirectory(_previousSitePath);
        }

        private void CreateOrChangeVirtualPicturesDirectory()
        {
            var app = _site.Applications[0];
            var virt = app.VirtualDirectories.FirstOrDefault(o => o.Path.Equals("/Pictures", StringComparison.OrdinalIgnoreCase));

            if (CurrentSite.AddPictures)
            {
                if (CurrentSite.LocalPictures)
                {
                    app.VirtualDirectories.Add("/Pictures", @"P:\\Pictures");
                }
                else
                {
                    var vdir = app.VirtualDirectories.Add("/Pictures", @"\\sc00\Images_Neu");
                    vdir.UserName = ServerSettings.Pictures.Username;
                    vdir.Password = ServerSettings.Pictures.Password;
                }
            }
            else
            {
                if (virt != null)
                {
                    app.VirtualDirectories.Remove(virt);
                }
            }
        }

        private void CreateOrChangeAppPool()
        {
            //Check Apppool
            if (!ServerManager.ApplicationPools.Any(o => o.Name.Equals(CurrentSite.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ServerManager.ApplicationPools.Add(CurrentSite.Name);
                //Create Appool                
                var apppool = ServerManager.ApplicationPools[CurrentSite.Name];
                apppool.ManagedRuntimeVersion = "v4.0";
                apppool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                apppool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;
                apppool.Recycling.PeriodicRestart.Time = TimeSpan.FromSeconds(0);
                apppool.Recycling.PeriodicRestart.Schedule.Add(TimeSpan.FromHours(1));
            }
        }

        private Site _site;
        private string _previousSitePath;
        private void CreateOrChangeWebsite()
        {
            _site = GetWebsite();
            if (_site == null)
                _site = ServerManager.Sites.Add(SiteName(), "http", "*:80:" + CurrentSite.Domains[0], GetSitePath());
            else
            {
                _previousSitePath = _site.Applications["/"].VirtualDirectories["/"].PhysicalPath;
                _site.Applications["/"].VirtualDirectories["/"].PhysicalPath = GetSitePath();
            }
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
