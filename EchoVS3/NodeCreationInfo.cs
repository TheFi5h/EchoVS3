using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EchoVS3
{
    [Serializable]
    public class NodeCreationInfo : ISerializable
    {
        public string Name { get; set; }
        public uint Size { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public List<IPEndPoint> Neighbors { get; set; }

        // Default constructor
        public NodeCreationInfo()
        {
            Neighbors = new List<IPEndPoint>();
        }

        public NodeCreationInfo(string name, uint size, string ip, int port, List<IPEndPoint> neighbors)
        {
            Name = name;
            Size = size;
            Ip = ip;
            Port = port;
            Neighbors = new List<IPEndPoint>(neighbors);
        }

        // Constructor for deserialization
        public NodeCreationInfo(SerializationInfo info, StreamingContext context)
        {
            Name = (string) info?.GetValue(nameof(Name), typeof(string)) ?? throw new ArgumentNullException(nameof(info));
            Size = (uint)info.GetValue(nameof(Size), typeof(uint));
            Ip = (string)info.GetValue(nameof(Ip), typeof(string));
            Port = (int)info.GetValue(nameof(Port), typeof(int));
            Neighbors = (List<IPEndPoint>) info.GetValue(nameof(Neighbors), typeof(List<IPEndPoint>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue($"{nameof(Name)}", Name);
            info.AddValue($"{nameof(Size)}", Size);
            info.AddValue($"{nameof(Ip)}", Ip);
            info.AddValue($"{nameof(Port)}", Port);
            info.AddValue($"{nameof(Neighbors)}", Neighbors);
        }

        public static byte[] MessageToByteArray(NodeCreationInfo nodeCreationInfo)
        {
            using (var ms = new MemoryStream())
            {
                try
                {
                    (new BinaryFormatter()).Serialize(ms, nodeCreationInfo);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: Error when parsing to byte array: {e.Message}");
                    return null;
                }

                return ms.ToArray();
            }
        }

        public static NodeCreationInfo FromByteArray(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                try
                {
                    return (NodeCreationInfo)(new BinaryFormatter()).Deserialize(ms);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: Error when parsing byte array to nodeCreationInfo: {e.Message}");
                    return null;
                }
            }
        }
    }
}
