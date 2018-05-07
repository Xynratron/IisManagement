using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;


namespace IisManagement.Server
{
    public static class Directory
    {
        public static bool Exists(string path)
        {
            using (new FileHandling(path))
            {
                return System.IO.Directory.Exists(path);
            }
        }
    }
}
