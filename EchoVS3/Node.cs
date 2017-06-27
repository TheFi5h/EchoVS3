using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EchoVS3
{
    public class Node
    {
        public string Name { get; }
        public uint Size { get; }
        public IPAddress IPAddress { get; }
        public int Port { get; }
        public IPEndPoint ParentNodeEndPoint { get; private set; }
        public UdpClient UdpClient { get; }

        public List<IPEndPoint> NeighborEndPoints = new List<IPEndPoint>();
        public readonly IPEndPoint LoggerEndPoint = new IPEndPoint(IPAddress.Parse("192.168.178.29"), 1234);

        private bool _continueListening = true;

        private const int ReceiveTimeout = 5000;

        // Constructors
        public Node(string name, uint size, IPEndPoint nodeEndPoint)
        {
            Name = name;
            Size = size;
            IPAddress = nodeEndPoint.Address;
            Port = nodeEndPoint.Port;
            UdpClient = new UdpClient(nodeEndPoint);
            UdpClient.Client.ReceiveTimeout = ReceiveTimeout;
        }

        public Node(NodeCreationInfo nodeCreationInfo)
        {
            Name = nodeCreationInfo.Name;
            Size = nodeCreationInfo.Size;

            IPAddress = IPAddress.Parse(nodeCreationInfo.Ip);
            Port = nodeCreationInfo.Port;

            // Create the udp client
            UdpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(nodeCreationInfo.Ip), nodeCreationInfo.Port));
            UdpClient.Client.ReceiveTimeout = ReceiveTimeout;

            // Add the neighbors
            NeighborEndPoints.AddRange(nodeCreationInfo.Neighbors);
        }

        // Methods
        // Will start to listen and receive messages until stopped
        public void Run()
        {
            bool isInformed = false;
            uint informedNeighbors = 0;
            uint receivedEchoSize = 0;

            while (_continueListening)
            {

                IPEndPoint receivedFromEndPoint = null;
                byte[] receivedBytes = { };

                try
                {
                    receivedBytes = UdpClient.Receive(ref receivedFromEndPoint);
                    Thread.Sleep(3000);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        // If the task is not set to be cancelled continue listening
                        if (_continueListening)
                            continue;

                        // Else -> cancel
                        return;
                    }
                    
                    Console.WriteLine($"Exception caught while trying to receive: {e.Message}");
                    throw;
                }
                Message message;

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
                        if (message.Data != "ECHO_START")
                            informedNeighbors++;

                        // If no parent set yet
                        if (ParentNodeEndPoint == null)
                        {
                            Log($"Parent set to {receivedFromEndPoint}.");
                            ParentNodeEndPoint = receivedFromEndPoint;
                        }

                        if (!isInformed)
                        {
                            Log($"Informed by {receivedFromEndPoint}.");

                            // Remember informed
                            isInformed = true;

                            // Send information message to all neighbors
                            SendToNeighbors(message);
                        }

                        Log($"Info received from {receivedFromEndPoint}.");

                        break;

                    case Type.Echo:

                        Log($"Echo received from {receivedFromEndPoint}.");

                        // Increment informed neighbors
                        informedNeighbors++;

                        // Remember data
                        receivedEchoSize += uint.Parse(message.Data);

                        break;

                    case Type.Logging:
                        throw new ArgumentException($"The given message type ({message.Type}) is not valid for this type of node", nameof(message.Type));

                    default:
                        throw new ArgumentException($"Unknown message type received: {message.Type}", nameof(message.Type));
                }

                // Check if all neighbors informed
                if (informedNeighbors == NeighborEndPoints.Count)
                {
                    message.Type = Type.Echo;
                    // Edit received message data
                    message.Data = (receivedEchoSize + Size).ToString();

                    // Send the message to the parent node
                    SendToParent(message);
                    Log($"Echo sent to parent: {ParentNodeEndPoint} with Data {message.Data}.");
                }
            }
        }

        public void Stop()
        {
            _continueListening = false;
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
            // Create copy to send
            Message newMessage = new Message(message.Type, message.Number, "");

            // Convert message to byte array
            byte[] messageByteArray = newMessage.ToByteArray();

            // Send message to all neighbors
            foreach (var neighbor in NeighborEndPoints)
            {
                // The parent node is not a normal neighbor
                if (neighbor.Address.Equals(ParentNodeEndPoint.Address) && neighbor.Port == ParentNodeEndPoint.Port)
                    continue;

                // Send UDP message
                UdpClient.Send(messageByteArray, messageByteArray.Length, neighbor);

                Log($"Info sent to: {neighbor} with Data {newMessage.Data}.");
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

            Printer.PrintLine($"{loggingMessage.Data}");
        }
    }
}