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
        public readonly IPEndPoint LoggerEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.105"), 1234);

        private bool _continueListening = true;

        private const bool UseReceiveTimeout = true;
        private const int MaxReceiveTimeout = 100;

        // Constructors
        public Node(string name, uint size, IPEndPoint nodeEndPoint)
        {
            Name = name;
            Size = size;
            IPAddress = nodeEndPoint.Address;
            Port = nodeEndPoint.Port;
            UdpClient = new UdpClient(nodeEndPoint);
            if(UseReceiveTimeout)
                UdpClient.Client.ReceiveTimeout = new Random((int)DateTime.Now.Ticks).Next(MaxReceiveTimeout);
        }

        public Node(NodeCreationInfo nodeCreationInfo)
        {
            Name = nodeCreationInfo.Name;
            Size = nodeCreationInfo.Size;

            IPAddress = IPAddress.Parse(nodeCreationInfo.Ip);
            Port = nodeCreationInfo.Port;

            // Create the udp client
            UdpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(nodeCreationInfo.Ip), nodeCreationInfo.Port));
            if (UseReceiveTimeout)
                UdpClient.Client.ReceiveTimeout = new Random((int)DateTime.Now.Ticks).Next(MaxReceiveTimeout);

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
                byte[] receivedBytes;

                try
                {
                    receivedBytes = UdpClient.Receive(ref receivedFromEndPoint);
                    Thread.Sleep(MaxReceiveTimeout);
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

                        Log($"{IPAddress}:{Port} recv INFO from {receivedFromEndPoint.Address}:{receivedFromEndPoint.Port} Data: \"{message.Data}\"");

                        // Increment informed neighbors
                        if (message.Data != "ECHO_START")
                            informedNeighbors++;

                        // If no parent set yet
                        if (ParentNodeEndPoint == null)
                        {
                            ParentNodeEndPoint = receivedFromEndPoint;
                        }

                        if (!isInformed)
                        {
                            // Remember informed
                            isInformed = true;

                            // Send information message to all neighbors
                            SendToNeighbors(message);
                        }


                        break;

                    case Type.Echo:

                        Log($"{IPAddress}:{Port} recv ECHO from {receivedFromEndPoint.Address}:{receivedFromEndPoint.Port} Data: \"{message.Data}\"");

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
                    Log($"{IPAddress}:{Port} send ECHO to {receivedFromEndPoint.Address}:{receivedFromEndPoint.Port} Data: \"{message.Data}\"");

                    Printer.PrintLine($"-----------------END OF RUN: {message.Number}-----------------");

                    // Reset states to be ready for a new run of the algorithm
                    isInformed = false;
                    informedNeighbors = 0;
                    receivedEchoSize = 0;
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

                Log($"{IPAddress}:{Port} send INFO to {neighbor.Address}:{neighbor.Port} Data: \"{newMessage.Data}\"");
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

            Printer.PrintLine($"{loggingMessage.Data}");
        }
    }
}