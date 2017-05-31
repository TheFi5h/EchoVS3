using System;
using System.Collections.Generic;
using System.Net;

namespace EchoVS3
{
    public class Node
    {
        public List<IPEndPoint> neighbors = new List<IPEndPoint>();

        private string _name;
        private uint _size;
        private string _ip;
        private uint _port;

        // Connection details to connect to logger
        private readonly IPEndPoint _loggerEndPoint = new IPEndPoint(IPAddress.Parse("192.168.2.2"), 1234);

        // Address of the parent node
        private readonly IPEndPoint _parentNodeEndPoint;

        // Sum of the communicated size of received echo messages
        private uint _receivedSize = 0;
        private uint _neighborsInformed;

        // Constructor
        public Node(string name, uint size, string ip, uint port)
        {
            _name = name;
            _size = size;
            _ip = ip;
            _port = port;
        }


        // Methods
        // Will be called, when a new message arrives
        public void ReactTo(Message message)
        {
            // Increment informed neighbors
            _neighborsInformed++;

            switch (message.Type)
            {
                case Type.Info:
                    if (_parentNodeEndPoint == null)
                    {
                        
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

            // Send message to logger
            
        }

        private void SendToNeighbors(Message message)
        {
            
        }
    }
}