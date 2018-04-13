namespace IisManagement.Shared
{
    public class DeleteWebsiteRequest
    {
        public string Name { get; set; }
        public bool KeepLocalFiles { get; set; }
    }
}