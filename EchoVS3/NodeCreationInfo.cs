using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace EchoVS3
{
    public class NodeCreationInfo : ISerializable
    {
        public string Name { get; set; }
        public uint Size { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public List<IPEndPoint> Neighbors { get; set; }

        // Default constructor
        public NodeCreationInfo() { }

        // Constructor for deserialization
        public NodeCreationInfo(string name, uint size, string ip, int port, List<IPEndPoint> neighbors)
        {
            Name = name;
            Size = size;
            Ip = ip;
            Port = port;
            Neighbors = new List<IPEndPoint>(neighbors);
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
    }
}
