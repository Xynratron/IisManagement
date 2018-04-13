using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using NLog;

namespace IisManagement.Client
{
    internal class ClientHandler : ChannelHandlerAdapter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        readonly IByteBuffer _initialMessage;

        public ClientHandler(string message)
        {
            _initialMessage = Unpooled.Buffer(256);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _initialMessage.WriteBytes(messageBytes);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            Logger.Debug("Start ChannelActive");

            Logger.Info("Sending Message");
            context.WriteAndFlushAsync(_initialMessage);

            Logger.Debug("End ChannelActive");
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Logger.Debug("Start ChannelRead");
            if (message is IByteBuffer byteBuffer)
            {
                var serverMessage = byteBuffer.ToString(Encoding.UTF8);
                Result = serverMessage;
                Logger.Info("Received from server: " + serverMessage);
            }
            Logger.Debug("Sending ServerAnswerArrivedevent");
            ServerAnswerArrived?.Invoke(this, new EventArgs());

            Logger.Debug("End ChannelRead");
        }
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Logger.Error(exception);
            context.CloseAsync();
        }

        public string Result { get; set; }
        public event EventHandler<EventArgs> ServerAnswerArrived;
    }
}