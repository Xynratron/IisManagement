
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IisAdmin
{
    class Program
    {    
        static void Main0(string[] args)
        {
            List<IisSite> sites = null;


            ServerHostsFile.CheckHostsFileForDefaultEntries();

            foreach (var site in sites.OrderBy(o => o.Name))
            {
                CreateSite(site);
                ServerHostsFile.CheckHostsFile(site);
            }
            ServerHostsFile.RemoveDuplicateEmptyHostEntries();
            Console.WriteLine("Finished");
        }

        static void CreateSite(IisSite newSite)
        {
            if (newSite == null)
                throw new ArgumentNullException("newSite");

            if (string.IsNullOrWhiteSpace(newSite.Name))
                throw new NullReferenceException("Der Name der Site wurde nicht angegeben");

            if (!newSite.Domains.Any())
                throw new Exception("Die Website benötigt mindestens eine Domain.");

            var serverManager = new ServerManager();

            //Site suchen
            var site = serverManager.Sites.FirstOrDefault(o => string.Equals(o.Name, newSite.Name, StringComparison.InvariantCultureIgnoreCase));

            //wenn nicht vorhanden, Anlegen
            if (site == null)
                site = serverManager.Sites.Add(newSite.Name, "http", "*:80:" + newSite.Domains[0], newSite.GetApplicationPath());

            //Assign Apppool
            var app = site.Applications[0];
            app.ApplicationPoolName = newSite.Name;

            //Add Virtual Dir
            if (!app.VirtualDirectories.Any(o => o.Path.Equals("/Pictures", StringComparison.OrdinalIgnoreCase)))
            {
                var vdir = app.VirtualDirectories.Add("/Pictures", @"\\sc00\Images_Neu");
                vdir.UserName = @"Username";
                vdir.Password = @"Password";
            }

            //Add Bindings
            foreach (var domain in newSite.Domains.Select(o => o.ToLowerInvariant()))
            {
                if (!site.Bindings.Any(o => string.Equals(o.Host, domain, StringComparison.InvariantCultureIgnoreCase)))
                    site.Bindings.Add("*:80:" + domain, "http");
            }
            var bindingRemoverList = new List<Binding>();
            //Remove obsolete Bindings
            foreach (var binding in site.Bindings)
            {
                if (!newSite.Domains.Any(o => o.Equals(binding.Host, StringComparison.InvariantCultureIgnoreCase)))
                {
                    bindingRemoverList.Add(binding);
                }
            }
            foreach (var binding in bindingRemoverList)
            {
                site.Bindings.Remove(binding);
            }

            
            bool mx1 = (newSite.AppKind == IisSiteApplicationKind.Mx1);
            
            //Check Apppool
            if (!serverManager.ApplicationPools.Any(o => o.Name.Equals(newSite.Name, StringComparison.OrdinalIgnoreCase)))
            {
                serverManager.ApplicationPools.Add(newSite.Name);
                //Create Appool                
                var apppool = serverManager.ApplicationPools[newSite.Name];
                apppool.ManagedRuntimeVersion = mx1? "v2.0" : "v4.0";
                apppool.ManagedPipelineMode = mx1 ? ManagedPipelineMode.Classic : ManagedPipelineMode.Integrated;
                apppool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;
                apppool.Recycling.PeriodicRestart.Time = TimeSpan.FromSeconds(0);
                apppool.Recycling.PeriodicRestart.Schedule.Add(TimeSpan.FromHours(1));
            }

            //Commit Changes
            serverManager.CommitChanges();

            if (!System.IO.Directory.Exists(newSite.GetApplicationPath()))
                System.IO.Directory.CreateDirectory(newSite.GetApplicationPath());
        }
    }

    public enum IisSiteApplicationKind
    {
        Mx1
    }

    public class IisSite
    {
        public string GetApplicationPath()
        {
            throw new NotImplementedException();
        }

        public string Name { get; set; }
        public List<string> Domains { get; set; }
        public IisSiteApplicationKind AppKind { get; set; }
    }
}
