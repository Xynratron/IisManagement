using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using IisManagement.Shared;
using NLog;
using NLog.Extensions.Logging;

namespace IisManagement.Client
{
    public static class Communication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static Communication()
        {
            InternalLoggerFactory.DefaultFactory.AddNLog();
        }

        private static async Task<string> SendMessageToServerAsync(string message)
        {
            var group = new MultithreadEventLoopGroup();
            var waitForServerHandle = new AutoResetEvent(false);

            try
            {
                var clientHandler = new ClientHandler(message);
                clientHandler.ServerAnswerArrived += (sender, args) =>
                {
                    waitForServerHandle.Set();
                };
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("handler", clientHandler);
                    }));

                var clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ClientSettings.Host), ClientSettings.Port));

                waitForServerHandle.WaitOne();
                
                await clientChannel.CloseAsync();
                return clientHandler.Result;
            }
            finally
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
        }

        private static string SendMessageToServer(string message)
        {
            Logger.Info("Sending new Message To Server");
            var t = SendMessageToServerAsync(message);
            var success = t.Wait(120000);
            if (!success)
            {
                Logger.Fatal("Connection to Server Timed Out");
                throw new TimeoutException();
            }
            Logger.Info("Result arrvied from Serevr");
            return t.Result;
        }

        public static TResult SendMessageToServer<TResult, TRequest>(TRequest message)
        {
            var serializedMessage = MessageConverter.Serialize(message);
            var result = SendMessageToServer(serializedMessage);
            return MessageConverter.DeSerialize<TResult>(result);
        }
    }
}