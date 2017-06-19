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
            }
            else if(Console.KeyAvailable)
            {
                // Create a plain node creation info object
                
                // Start reading the inputs

                Console.WriteLine("Bitte eigene IP angeben: ");
                Console.ReadLine();

                Console.WriteLine("Bitte Port angeben: ");
                Console.ReadLine();

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
