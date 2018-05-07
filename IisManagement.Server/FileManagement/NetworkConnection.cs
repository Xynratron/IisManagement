using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace IisManagement.Server.FileManagement
{
    /// <summary>
    /// Connection to a Network Share with different Credentials. 
    /// If you have to connect to another Computer with different Credentials e.G. if you have no 
    /// Domain Account, you can use this helper functionn
    /// </summary>
    /// <example>
    /// The Class has IDisposable, so we have tuo use <c>using</c>. the Connection is established 
    /// during the Constructor and released with the implicit call to dispose.
    /// The example copies a local File to a Server with the Adminstrative "c$" Share.
    /// <code>
    /// var credentials = new NetworkCredential(Settings.Default.Username, Settings.Default.Password, Settings.Default.Domain);
    /// var serverPath = @"\\192.168.10.20\c$\Temp";
    /// using (new NetworkConnection(Path.GetPathRoot(serverPath), credentials))
    /// {
    ///     File.Copy(@"c:\temp\localFile.xml", Path.Combine(serverPath, "ServerFile.xml");
    /// }
    /// </code>
    /// </example>
    public class NetworkConnection : IDisposable
    {
        readonly string _networkName;

        /// <summary>
        /// Create an Insance of NetworkConnection to another computer using different credentials
        /// </summary>
        /// <param name="networkName">the remote computer / share</param>
        /// <param name="credentials">Network credentials to use, with or without domain</param>
        public NetworkConnection(string networkName, NetworkCredential credentials)
        {
            _networkName = networkName;

            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = networkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName
                : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

            var result = WNetAddConnection2(
                netResource,
                credentials.Password,
                userName,
                0);

            if (result != 0 && result != 1219)
            {
                var sbErrorBuf = new StringBuilder(500);
                var sbNameBuf = new StringBuilder(500);

                WNetGetLastError(ref result, sbErrorBuf, sbErrorBuf.Capacity, sbNameBuf, sbNameBuf.Capacity);
                throw new Exception(string.Format("Error while Connection to Taget-Server: {0} \r\n from Provider {1} ",
                    sbErrorBuf, sbNameBuf));
            }
        }

        ~NetworkConnection()
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
            WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll", SetLastError = true)]
        private static extern int WNetAddConnection2(NetResource netResource, string password, string username,
            int flags);

        [DllImport("mpr.dll", SetLastError = true)]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        [DllImport("mpr.dll", SetLastError = true)]
        private static extern int WNetGetLastError(ref int lpError, StringBuilder lpErrorBuf, int nErrorBufSize,
            StringBuilder lpNameBuf, int nNameBufSize);

        [StructLayout(LayoutKind.Sequential)]
        private class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        private enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        };

        private enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        private enum ResourceDisplaytype : int
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }
    }
}