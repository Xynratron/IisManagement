namespace IisManagement.Server.Settings
{
    public class ServerSettings
    {
        public static int Port { get; set; }

        public static string BasePath { get; set; }
        
        public static Pictures Pictures { get; set; }
        public static Deployment Deployment { get; set; }
    }
}