using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IisManagement.Shared;

namespace IisAdmin
{
    public class ServerHostsFile
    {
        private static readonly List<string> ServerHostsDefaultEntries = new List<string>(new[]
        {
            @"192.168.10.80	images002.bmf-application.com",
            @"192.168.10.80	images001.bmf-application.com",
            @"192.168.10.80	gutachten.bmf-application.com", 
            @"192.168.10.80	www.gutachten.bmf-application.com"
        });
        
        private static void CheckHostsFileForDefaultEntries()
        {
            //Alle Datensätze einlesen in Array
            var hosts = File.ReadAllLines(Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts")).ToList();

            //Check Default Entries.
            string marker = "Server Default";

            CheckHostsForDomains(hosts, ServerHostsDefaultEntries, marker);
            
            File.WriteAllLines(Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts"), hosts);
        }

        public static void CheckHostsFile(IisSite newSite)
        {
            CheckHostsFileForDefaultEntries();

            var hosts = File.ReadAllLines(Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts")).ToList();
          
            //Check Default Entries.
            string marker = string.Format("{0} - {1}", newSite.SiteName, newSite.Group);
            
            var domainEntries = newSite.Domains.Select(o => $"127.0.0.1\t{o}").ToList();

            CheckHostsForDomains(hosts, domainEntries, marker);

            File.WriteAllLines(Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts"), hosts);
        }

        public static void RemoveDuplicateEmptyHostEntries()
        {
            var hosts = File.ReadAllLines(Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts")).ToList();

            bool lastWasEmpty = false;
            for (int i = hosts.Count - 1; i >0 ; i--)
            {
                if (lastWasEmpty && string.IsNullOrWhiteSpace(hosts[i]))
                {
                    hosts.RemoveAt(i);
                    continue;
                }
                lastWasEmpty = string.IsNullOrWhiteSpace(hosts[i]);
            }
            File.WriteAllLines(Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts"), hosts);
        }

        private static void CheckHostsForDomains(List<string> hosts, List<string> domainentries, string marker)
        {
            var start = hosts.IndexOf(GetStartMarker(marker));
            var ende = hosts.IndexOf(GetEndMarker(marker));

            if (start == -1)
            {
                RemoveFromHosts(hosts, domainentries, ende);                
                AddEntries(hosts, domainentries, marker);                
            }
            else
            {
                if (ende > start)
                {
                    hosts.RemoveRange(start, ende - start + 1);
                }
                else
                {
                    RemoveFromHosts(hosts, domainentries, start);
                }

                AddEntries(hosts, domainentries, marker);                
            }
        }

        private static string GetStartMarker(string marker)
        {
            return "# " + marker + " Entries";
        }

        private static string GetEndMarker(string marker)
        {
            return "#/ " + marker + " Entries";
        }

        private static void AddEntries(List<string> hosts, List<string> domainEntries, string markerComment)
        {
            hosts.Add("");
            hosts.Add(GetStartMarker(markerComment));           
            hosts.AddRange(domainEntries);            
            hosts.Add(GetEndMarker(markerComment));
            hosts.Add("");
        }

        private static void RemoveFromHosts(List<string> hosts, List<string> domainEntries, int marker = -1)
        {
            if (marker >= 0)
                hosts.RemoveAt(marker);
            //evtl. schon vorhandene Einträge entfernen
            foreach (var defaultEntry in domainEntries)
                hosts.Remove(defaultEntry);

            var ips = domainEntries.Select(o => o.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                .Where(o => o.Length > 1)
                .Select(o=> o[1]);             
            
            foreach (var defaultHost in hosts.Where(o =>
            {
                var splits = o.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (o.Length < 2)
                    return false;
                return ips.Contains(splits[1]);
            }).ToList())
            {
                hosts.Remove(defaultHost);
            }
        }
    }
}