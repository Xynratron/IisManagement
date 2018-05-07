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
                Logger.Info($"Manipulating Hosts File");
                ManipulateHostsFile();
                Logger.Info($"Changing Website");
                ChangeWebsite();
                Logger.Info($"Copy Website Contents");
                CopyContents();
                Logger.Info($"Commiting all Changes");
                ServerManager.CommitChanges();
                Logger.Info($"Deleting old Directory");
                RemoveOldDirectory();
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
            if (_previousSitePath != GetSitePath() && !string.IsNullOrWhiteSpace(_previousSitePath))
            {
                Logger.Info("New Version of Site due to Path-Change detected");

                Logger.Info($"Old Path: {_previousSitePath}");
                Logger.Info($"New Path: {GetSitePath()}");
                Logger.Info($"Searching for new New Version at Deployment Location {GetDeploymentPath()}");

                if (!Directory.Exists(GetDeploymentPath()))
                {
                    Logger.Info($"Could not Find New Version of Site");

                    Directory.CreateDirectory(GetDeploymentPath());
                    
                    Logger.Info($"Copy Old Site to Deployment as initial Version");
                    CopyFilesRecursively(_previousSitePath, GetDeploymentPath());
                    
                }
                Logger.Info($"Copy Site from Deployment to local");
                CopyFilesRecursively(GetDeploymentPath(), GetSitePath());
                Logger.Info($"Finished Copy Site");
            }
        }

        private void ChangeWebsite()
        {
            EnsureSiteDirecory();
            CreateOrChangeWebsite();
            CreateOrChangeAppPool();
            CreateOrChangeVirtualPicturesDirectory();
            AddOrChangeBindings();
        }

        private void AddOrChangeBindings()
        {
            Logger.Info($"Adding Binding information");
            //Add Bindings
            foreach (var domain in CurrentSite.Domains.Select(o => o.ToLowerInvariant()))
            {
                Logger.Info($"Domain: {domain}");
                if (!_site.Bindings.Any(o => string.Equals(o.Host, domain, StringComparison.InvariantCultureIgnoreCase)))
                    _site.Bindings.Add("*:80:" + domain, "http");
            }
            
            //Remove obsolete Bindings
            foreach (var binding in _site.Bindings.ToList())
            {
                Logger.Info($"Searching for Bindings to be removed");
                if (!CurrentSite.Domains.Any(o => o.Equals(binding.Host, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Logger.Info($"binding: {binding}");
                    _site.Bindings.Remove(binding);
                }
            }
        }

        private void RemoveOldDirectory()
        {
            if (string.IsNullOrWhiteSpace(_previousSitePath))
                return;
            if (Directory.Exists(_previousSitePath))
                Directory.Delete(_previousSitePath);
        }

        private void CreateOrChangeVirtualPicturesDirectory()
        {
            Logger.Info($"Checking Virtual Pictures Folders");
            var app = _site.Applications[0];
            
            var virt = app.VirtualDirectories.FirstOrDefault(o => o.Path.Equals("/Pictures", StringComparison.OrdinalIgnoreCase));
            if (virt != null)
            {
                Logger.Info($"Remove the existing Virtual Directory");
                app.VirtualDirectories.Remove(virt);
            }

            if (CurrentSite.AddPictures)
            {
                Logger.Info($"Adding new Virtual Directory");
                if (CurrentSite.LocalPictures)
                {
                    Logger.Info($"As Local Pictures");
                    app.VirtualDirectories.Add("/Pictures", @"P:\\Pictures");
                }
                else
                {
                    Logger.Info($"As Server Pictures");
                    var vdir = app.VirtualDirectories.Add("/Pictures", @"\\sc00\Images_Neu");
                    vdir.UserName = ServerSettings.Pictures.Username;
                    vdir.Password = ServerSettings.Pictures.Password;
                }
            }
        }

        private void CreateOrChangeAppPool()
        {
            Logger.Info($"Checking Application Pool");
            var app = _site.Applications[0];

            ApplicationPool apppool = null;
            var renameAppPoool = app.ApplicationPoolName != SiteName();

            if (_siteIsNew || renameAppPoool)
            {
                Logger.Info($"Adding a new Pool");
                apppool = ServerManager.ApplicationPools.Add(SiteName());
            }
            else
            {
                Logger.Info($"Using existing Pool");
                apppool = ServerManager.ApplicationPools[app.ApplicationPoolName];
            }

            Logger.Info($"Settings the default values");

            apppool.ManagedRuntimeVersion = "v4.0";
            apppool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
            apppool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;
            apppool.Recycling.PeriodicRestart.Time = TimeSpan.FromSeconds(0);
            apppool.Recycling.PeriodicRestart.Schedule.Clear();
            apppool.Recycling.PeriodicRestart.Schedule.Add(TimeSpan.FromHours(1));
            
            Logger.Info($"Settings Pool for Website-Application");
            app.ApplicationPoolName = SiteName();
        }

        private Site _site;
        private string _previousSitePath;
        private bool _siteIsNew = false;
        private void CreateOrChangeWebsite()
        {
            Logger.Info($"Searching for Website {SiteName()}");
            _site = GetWebsite();
            if (_site == null)
            {
                Logger.Info($"Adding Website {SiteName()}");
                Logger.Info($"For Domain {CurrentSite.Domains[0]}");
                Logger.Info($"With Path {GetSitePath()}");
                _site = ServerManager.Sites.Add(SiteName(), "http", "*:80:" + CurrentSite.Domains[0], GetSitePath());
                _siteIsNew = true;
            }
            else
            {
                Logger.Info($"Website allready exists");
                Logger.Info($"Searching for physical Path");
                _previousSitePath = _site.Applications["/"].VirtualDirectories["/"].PhysicalPath;
                Logger.Info($"Physical Path is {_previousSitePath}");
            }
            _site.Applications["/"].VirtualDirectories["/"].PhysicalPath = GetSitePath();
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
