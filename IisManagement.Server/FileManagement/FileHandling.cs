using System;
using System.Net;
using IisManagement.Server.Settings;
using NLog;

namespace IisManagement.Server.FileManagement
{
    public class FileHandling : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private NetworkConnection _networkConnection;

        public FileHandling(string path)
        {
            if (IsNetworkShare(path))
            {
                var settings = ServerSettings.Deployment;
                _networkConnection = new NetworkConnection(System.IO.Path.GetPathRoot(path),
                    new NetworkCredential(settings.Username, settings.Password, settings.Domain));
            }
        }

        private bool IsNetworkShare(string path)
        {
            var needImpoersonation = path.StartsWith(@"\\");
            Logger.Info($"we need Impersonation: {needImpoersonation}");
            return needImpoersonation;
        }

        ~FileHandling()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _networkConnection?.Dispose();
            _networkConnection = null;
        }
    }
}