using System;
using System.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EchoVS3
{
    [Serializable]
    public class Message : ISerializable
    {
        private static BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public Message(Type type, uint number, string data)
        {
            Type = type;
            Number = number;
            Data = data;
        }

        // Constructor to Deserialize
        public Message(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            Type = (Type) info.GetValue($"{nameof(Type)}", typeof(Type));
            Number = (uint) info.GetValue($"{nameof(Number)}", typeof(uint));
            Data = (string) info.GetValue($"{nameof(Data)}", typeof(string));
        }

        public Type Type { get; set; }

        public uint Number { get; set; }

        public string Data { get; set; }

        // Method so that the serializer knows how to serialize
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
                throw new ArgumentNullException("info");

            info.AddValue($"{nameof(Type)}", Type);
            info.AddValue($"{nameof(Number)}", Number);
            info.AddValue($"{nameof(Data)}", Data);
        }
    }
}