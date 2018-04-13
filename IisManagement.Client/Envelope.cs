using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IisManagement.Shared
{
    [DataContract]
    public class Envelope
    {
        [DataMember]
        private string MessageTypeString { get; set; }

        [DataMember]
        private string JsonMessageString { get; set; }

        private static readonly ConcurrentDictionary<string, Type> ResolvedTypes = new ConcurrentDictionary<string, Type>();
     
        private Type _messageType;

        public Type MessageType
        {
            get
            {
                if (_messageType == null)
                {
                    if (!string.IsNullOrWhiteSpace(MessageTypeString))
                    {
                        if (!ResolvedTypes.TryGetValue(MessageTypeString, out _messageType))
                        {
                            _messageType = ResolvedTypes.GetOrAdd(MessageTypeString, s =>
                                Type.GetType(MessageTypeString) ??
                                Type.GetType(MessageTypeString,
                                    name => AppDomain.CurrentDomain.GetAssemblies()
                                        .FirstOrDefault(z => z.FullName == name.FullName), null)
                            );
                        }
                    }
                }
                return _messageType;
            }
            set
            {
                if (_messageType == value) return;
                MessageTypeString = value.AssemblyQualifiedName;
                _messageType = value;
            }
        }

        private dynamic _message;

        public dynamic Message
        {
            get
            {
                if (_message == null)
                {
                    if (JsonMessageString != null)
                    {
                        _message = JsonConvert.DeserializeObject(JsonMessageString, MessageType);
                    }
                }
                return _message;
            }
            set
            {
                if (_message == null || _message != value)
                {
                    MessageType = value.GetType();
                    JsonMessageString = JsonConvert.SerializeObject(value);
                    _message = value;
                }
            }
        }
    }
}