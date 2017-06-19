using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EchoVS3
{
    class NodeCreationInfo : ISerializable
    {
        public string Name { get; set; }
        public uint Size { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public List<Tuple<string, int>> Neighbors { get; set; }

        // Default constructor
        public NodeCreationInfo() { }

        // Constructor for deserialization
        public NodeCreationInfo(string name, uint size, string ip, int port, List<Tuple<string, int>> neighbors)
        {
            Name = name;
            Size = size;
            Ip = ip;
            Port = port;
            Neighbors = new List<Tuple<string, int>>(neighbors);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue($"{nameof(Name)}", Name);
            info.AddValue($"{nameof(Size)}", Size);
            info.AddValue($"{nameof(Ip)}", Ip);
            info.AddValue($"{nameof(Port)}", Port);
        }
    }
}
