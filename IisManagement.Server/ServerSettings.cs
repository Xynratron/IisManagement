using System.Security.Policy;
using NLog;

namespace IisManagement.Server
{
    public class ServerSettings
    {
        public static int Port { get; set; }

        public static string BasePath { get; set; }
        
        public static Pictures Pictures { get; set; }
    }
    public class Pictures
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}