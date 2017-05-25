using System.Collections.Generic;
using System.Net;

namespace EchoVS3
{
    public class Node
    {
        public readonly List<IPEndPoint> NeighborEndPoints = new List<IPEndPoint>();
        public readonly IPEndPoint NodeEndPoint;
        public readonly IPEndPoint ParentNodeEndPoint;
        public readonly IPEndPoint LoggerEndPoint = new IPEndPoint(IPAddress.Parse("192.168.2.2"), 1234);

        public readonly string Name;
        public readonly uint Size;
        
        private uint _receivedEchoSize = 0;
        private bool _isInformed = false;
        private uint _informedNeighbors = 0;

        // Constructor
        public Node(string name, uint size, IPEndPoint nodeEndPoint)
        {
            Name = name;
            Size = size;
            NodeEndPoint = nodeEndPoint;
        }

        // Methods
        // Will be called, when a new message arrives
        public void ReactTo(Message message)
        {
            // Filter by type
        }
    }
}