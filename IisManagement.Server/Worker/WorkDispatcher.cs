using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IisManagement.Shared;
using NLog;

namespace IisManagement.Server.Worker
{
    public static class WorkDispatcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<Type, Type> _consumers = new Dictionary<Type, Type>();

        static WorkDispatcher()
        {
            var loadedTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());

            foreach (var workerType in loadedTypes)
            {
                var typeInterface = workerType
                    .GetInterfaces()
                    .FirstOrDefault(o =>
                        o.IsGenericType &&
                        (o.GetGenericTypeDefinition() == typeof(IWorker<,>))
                    );
                if (typeInterface == null)
                    continue;
                Type consumerType = typeInterface.GetGenericArguments()[0];
                _consumers.Add(consumerType, workerType);
            }
            if (_consumers.Any())
            {
                Logger.Info($"Found {_consumers.Count} consumers:");
                foreach (var consumer in _consumers)
                {
                    Logger.Info($"{consumer.Value} for {consumer.Key}");
                }
            }
            else
            {
                Logger.Fatal($"Found No consumers.");
            }
        }

        public static string DoIt(string message)
        {
            var envelope = MessageConverter.DeSerialize(message);

            Logger.Info($"Searching Consumer for {envelope.MessageType}");

            if (envelope.MessageType != null && _consumers.TryGetValue(envelope.MessageType, out var consumer))
            {
                Logger.Info($"Found Consumer {consumer}.");
                var o = Activator.CreateInstance(consumer);

                Logger.Info($"Searching Working-Method");
                MethodInfo method = consumer.GetMethod("ReceiveAndSendMessage");
                
                if (method != null)
                {
                    Logger.Info($"Invoking Worker");
                    var result = method.Invoke(o, new object[] { envelope.Message });

                    Logger.Info("Returning Result");
                    return MessageConverter.Serialize(result);
                }
                Logger.Info($"No Working-Method Found!");
            }

            Logger.Info($"No Consumer found.");
            return MessageConverter.Serialize(new NoWorkerForRequestFound());
        }
    }
}
