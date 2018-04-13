namespace IisManagement.Server.Worker
{
    public interface IWorker<in T, R> 
    {
        R ReceiveAndSendMessage(T message);

    }
}