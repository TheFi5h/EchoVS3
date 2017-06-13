using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EchoVS3
{
    [Serializable]
    public class Message : ISerializable
    {
        public Type Type { get; set; }

        public uint Number { get; set; }

        public string Data { get; set; }


        private static readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();


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
                throw new ArgumentNullException(nameof(info));

            Type = (Type) info.GetValue($"{nameof(Type)}", typeof(Type));
            Number = (uint) info.GetValue($"{nameof(Number)}", typeof(uint));
            Data = (string) info.GetValue($"{nameof(Data)}", typeof(string));
        }

        // Method so that the serializer knows how to serialize
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if(info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue($"{nameof(Type)}", Type);
            info.AddValue($"{nameof(Number)}", Number);
            info.AddValue($"{nameof(Data)}", Data);
        }


        public byte[] ToByteArray()
        {
            return Message.MessageToByteArray(this);
        }

        public static byte[] MessageToByteArray(Message message)
        {
            using (var ms = new MemoryStream())
            {
                try
                {
                    _binaryFormatter.Serialize(ms, message);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: Error when parsing to byte array: {e.Message}");
                    return null;
                }

                return ms.ToArray();
            }
        }

        public static Message FromByteArray(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                try
                {
                    return (Message) _binaryFormatter.Deserialize(ms);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: Error when parsing byte array to message: {e.Message}");
                    return null;
                }
            }
        }
    }
}