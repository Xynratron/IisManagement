namespace IisManagement.Shared
{
    public class CreateBackupRequest
    {
        public string Group { get; set; }
        public string SiteName { get; set; }
        public string Version { get; set; }
    }
}