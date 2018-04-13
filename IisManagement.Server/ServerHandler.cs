using System;
using System.Collections.Concurrent;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using IisManagement.Server.Worker;
using NLog;

namespace IisManagement.Server
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer buffer)
            {
                var clientMessage = buffer.ToString(Encoding.UTF8);
                Logger.Debug("Received from client: " + clientMessage);

                var result = WorkDispatcher.DoIt(clientMessage);

                Logger.Info("Returning Working Result");
                Logger.Debug(result);

                var resultBuffer = Unpooled.Buffer(256);
                byte[] messageBytes = Encoding.UTF8.GetBytes(result);
                resultBuffer.WriteBytes(messageBytes);

                context.WriteAsync(resultBuffer);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Logger.Error(exception);
            context.CloseAsync();
        }
    }
}