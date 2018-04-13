using System.Diagnostics;
using Newtonsoft.Json;

namespace IisManagement.Shared
{
    public class MessageConverter
    {
        public static string Serialize<T>(T message)
        {
            var messageEnvelope = new Envelope
            {
               
                MessageType = typeof(T),
                Message = message
            };
            return JsonConvert.SerializeObject(messageEnvelope);
        }

        public static T DeSerialize<T>(string message)
        {
            var messageEnvelope = JsonConvert.DeserializeObject<Envelope>(message);
            return (T) messageEnvelope.Message;
        }
        public static Envelope DeSerialize(string message)
        {
            return JsonConvert.DeserializeObject<Envelope>(message);
        }
    }
}
