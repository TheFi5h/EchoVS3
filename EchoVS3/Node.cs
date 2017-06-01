using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EchoVS3
{
    public class Node
    {
        public UdpClient UdpClient { get; set; }
        public IPEndPoint ParentNodeEndPoint { get; set; }

        public readonly List<IPEndPoint> NeighborEndPoints = new List<IPEndPoint>();
        public readonly IPEndPoint NodeEndPoint;
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
            UdpClient = new UdpClient(NodeEndPoint);
        }

        // Methods
        // Will be called, when a new message arrives
        public void ReactTo(IPEndPoint receivedFromEndPoint, Message message)
        {
            _isInformed = true;

            Log($"Node {Name} informed by {receivedFromEndPoint}.");


            switch (message.Type)
            {
                case Type.Info:
                    // Increment informed neighbors
                    _informedNeighbors++;

                    // If no parent set yet
                    if (ParentNodeEndPoint == null)
                    {
                        ParentNodeEndPoint = receivedFromEndPoint;
                    }
                    break;
                case Type.Echo:

                    break;
                case Type.Logging:

                        break;
                default:
                    Console.WriteLine("Unknown message type received: {0}", message.Type);
                    break;
            }
        }

        // Sends a message to the logger
        private void Log(string message)
        {
            // Build message package
            Message loggingMessage = new Message(Type.Logging, 0, message);

            // Convert message to byte array
            byte[] messageByteArray = loggingMessage.ToByteArray();

            // Send message to logger
            UdpClient.Send(messageByteArray, messageByteArray.Length, LoggerEndPoint);
        }

        // Sends a message to all neighbors
        private void SendToNeighbors(Message message)
        {
            // Convert message to byte array
            byte[] messageByteArray = message.ToByteArray();

            // Send message to all neighbors
            foreach(var neighbor in NeighborEndPoints)
            {
                // Send UDP message
                UdpClient.Send(messageByteArray, messageByteArray.Length, neighbor);
            }
        }
    }
}