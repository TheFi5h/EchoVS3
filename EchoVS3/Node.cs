using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EchoVS3
{
    public class Node
    {
        public string Name { get; }
        public uint Size { get; }
        public IPEndPoint ParentNodeEndPoint { get; private set; }
        public UdpClient UdpClient { get; }

        public List<IPEndPoint> NeighborEndPoints = new List<IPEndPoint>();
        public readonly IPEndPoint LoggerEndPoint = new IPEndPoint(IPAddress.Parse("192.168.2.2"), 1234);

        private bool _continueListening = true;

        // Constructor
        public Node(string name, uint size, IPEndPoint nodeEndPoint)
        {
            Name = name;
            Size = size;
            UdpClient = new UdpClient(nodeEndPoint);
        }

        // Methods
        // Will start to listen and receive messages until stopped
        private void Run()
        {
            bool isInformed = false;
            uint informedNeighbors = 0;
            uint receivedEchoSize = 0;

            while (_continueListening)
            {
                IPEndPoint receivedFromEndPoint = null;
                var receivedBytes = UdpClient.Receive(ref receivedFromEndPoint);
                Message message = null;

                try
                {
                    // Try convert received bytes to message
                    message = Message.FromByteArray(receivedBytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Byte array could not be converted to a message: " + e.Message);
                    continue;
                }

                switch (message.Type)
                {
                    case Type.Info:

                        // Increment informed neighbors
                        informedNeighbors++;

                        if (!isInformed)
                        {
                            Log($"Informed by {receivedFromEndPoint}.");

                            // Remember informed
                            isInformed = true;

                            // Send information message to all neighbors
                            SendToNeighbors(message);
                        }

                        // If no parent set yet
                        if (ParentNodeEndPoint == null)
                        {
                            Log($"Parent set to {receivedFromEndPoint}.");
                            ParentNodeEndPoint = receivedFromEndPoint;
                        }

                        break;

                    case Type.Echo:

                        Log($"Echo received from {receivedFromEndPoint}.");

                        // Increment informed neighbors
                        informedNeighbors++;

                        // Remember data
                        receivedEchoSize += uint.Parse(message.Data);

                        // Check if all neighbors informed
                        if (informedNeighbors == NeighborEndPoints.Count)
                        {
                            // Edit received message data
                            message.Data = (receivedEchoSize + Size).ToString();

                            // Send the message to the parent node
                            SendToParent(message);
                        }

                        break;

                    case Type.Logging:
                        throw new ArgumentException($"The given message type ({message.Type}) is not valid for this type of node", nameof(message.Type));

                    default:
                        throw new ArgumentException($"Unknown message type received: {message.Type}", nameof(message.Type));
                }
            }
        }

        // Sends a message to the parent node
        private void SendToParent(Message message)
        {
            // Convert to byte array
            var array = message.ToByteArray();

            // Check if parent node is set
            if (ParentNodeEndPoint == null)
                throw new NullReferenceException("Echo received, but parent node is null.");

            // Send echo message to parent
            UdpClient.Send(array, array.Length, ParentNodeEndPoint);
        }

        // Sends a message to all neighbors
        private void SendToNeighbors(Message message)
        {
            // Convert message to byte array
            byte[] messageByteArray = message.ToByteArray();

            // Send message to all neighbors
            foreach (var neighbor in NeighborEndPoints)
            {
                // Send UDP message
                UdpClient.Send(messageByteArray, messageByteArray.Length, neighbor);
            }
        }

        // Sends a message to the logger
        private void Log(string message)
        {
            // Build message package
            Message loggingMessage = new Message(Type.Logging, 0, $"Node {Name}: " + message);

            // Convert message to byte array
            byte[] messageByteArray = loggingMessage.ToByteArray();

            // Send message to logger
            UdpClient.Send(messageByteArray, messageByteArray.Length, LoggerEndPoint);
        }
    }
}