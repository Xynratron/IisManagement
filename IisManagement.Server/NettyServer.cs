using System;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using IisManagement.Server.Settings;
using NLog.Extensions.Logging;

namespace IisManagement.Server
{
    internal class NettyServer
    {
        public NettyServer()
        {
            InternalLoggerFactory.DefaultFactory.AddNLog();
        }

        private IEventLoopGroup _bossGroup;
        private IEventLoopGroup _workerGroup;
        private IChannel _boundChannel;

        private async Task StartAsync()
        {
            _bossGroup = new MultithreadEventLoopGroup(1);
            _workerGroup = new MultithreadEventLoopGroup();

            var bootstrap = new ServerBootstrap();
            bootstrap.Group(_bossGroup, _workerGroup);

            bootstrap.Channel<TcpServerSocketChannel>();

            bootstrap
                .Option(ChannelOption.SoBacklog, 100)
                .Handler(new LoggingHandler("SRV-LSTN"))
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                    pipeline.AddLast("echo", new ServerHandler());
                }));

            _boundChannel = await bootstrap.BindAsync(ServerSettings.Port);
        }

        private async Task StopAsync()
        {
            await _boundChannel.CloseAsync();
            await Task.WhenAll(
                _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
        }

        public void Start()
        {
            StartAsync().Wait();
        }

        public void Stop()
        {
            StopAsync().Wait();
        }
    }
}