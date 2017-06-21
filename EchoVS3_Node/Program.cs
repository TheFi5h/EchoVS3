using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using EchoVS3;
using System.Threading;

namespace EchoVS3_Node
{
    class Program
    {
        private static Node _node;
        private static int configurationPort = 9999;

        private static byte[] receivedBytes;
        private static bool messageReceived = true;

        static void Main(string[] args)
        {
            // Start to listen on the configuration port for configuration messages
            var ipEndPoint = new IPEndPoint(IPAddress.Any, configurationPort);
            var udpClient = new UdpClient(ipEndPoint);
            var udpState = new UdpState(udpClient, ipEndPoint);

            // Start listening to the configuration port
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), new UdpState(udpClient, ipEndPoint));

            // Start listening for keyboard input
            do
            {
                // Will wait for either a message received or a key pressed
                Thread.Sleep(100);

            } while (messageReceived != true && Console.KeyAvailable != true);

            // Check what has been triggered
            if(messageReceived)
            {
                // Convert received bytes to an node creation info object
                var nodeCreationInfo = NodeCreationInfo.FromByteArray(receivedBytes);

                // Create the node with the given info
                _node = new Node(nodeCreationInfo);

            }
            else if(Console.KeyAvailable)
            {
                // Stop listening for udp messages
                // Will trigger the EndReceive method but udp
                try
                {
                    udpClient.Close();
                }
                catch (Exception)
                {
                    //MSDN suggests using Close method to cancel receive, but this will throw an ObjectDisposedException because
                    // the EndReceive method is called on the disposed object
                    //https://msdn.microsoft.com/en-us/library/dxkwh6zw.aspx
                }

                // Create a plain node creation info object
                NodeCreationInfo nodeCreationInfo = new NodeCreationInfo();

                // Start reading the inputs
                Console.Write("Bitte eigene IP angeben: ");
                nodeCreationInfo.Ip = Console.ReadLine();

                Console.Write("Bitte Port angeben: ");
                nodeCreationInfo.Port = int.Parse(Console.ReadLine());

                Console.Write("Bitte einen Namen für den Knoten eingeben: ");
                nodeCreationInfo.Name = Console.ReadLine();

                Console.Write("Bitte die Größe des Knotens angeben: ");
                nodeCreationInfo.Size = uint.Parse(Console.ReadLine());

                // Get n-times neighbor information
                string input = "";

                while(true)
                {
                    // Get ip for neighbor
                    Console.Write("Bitte IP für Nachbar angeben (Hinzufügen beenden mit Enter ohne Eingabe): ");
                    input = Console.ReadLine();

                    // Check if enter pressed without input
                    if (input == string.Empty)
                        break;

                    // Get port of neighbor
                    Console.Write("Bitte Port für Nachbar angeben: ");
                    var neighborEndPoint = new IPEndPoint(IPAddress.Parse(input), int.Parse(Console.ReadLine()));

                    // Add the endpoint to the list
                    nodeCreationInfo.Neighbors.Add(neighborEndPoint);
                }

            }
        }

        private static void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                // Get the udp client saved in the udp state
                var udpClient = ((UdpState)result.AsyncState).UdpClient;
                var ipEndPoint = ((UdpState)result.AsyncState).IPEndPoint;

                // Check what has been received and end receiving
                receivedBytes = udpClient.EndReceive(result, ref ipEndPoint);
                messageReceived = true;
            }
            catch(Exception)
            {
                // Catch the exception if the receiving was cancelled from outside
            }
        }

        private class UdpState
        {
            public UdpClient UdpClient { get; }
            public IPEndPoint IPEndPoint { get; }

            public UdpState(UdpClient udpClient, IPEndPoint ipEndPoint)
            {
                UdpClient = udpClient;
                IPEndPoint = ipEndPoint;
            }
        }
    }
}
