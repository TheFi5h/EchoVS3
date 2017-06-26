using EchoVS3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace EchoVS3_NodeNetworkCreator
{
    class Program
    {
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();

            const int configurationPort = 9999;

            UdpClient udpClient = null;

            string nodeAIp = "192.168.0.105";
            string nodeBIp = "192.168.0.105";
            string nodeCIp = "192.168.0.105";
            string nodeDIp = "192.168.0.105";

            int nodeAPort = 1111;
            int nodeBPort = 2222;
            int nodeCPort = 3333;
            int nodeDPort = 4444;

            // Initialize list of creation infos
            List<NodeCreationInfo> nodeCreationInfos = new List<NodeCreationInfo>
            {
                new NodeCreationInfo(name: "A", size: 3, ip: nodeAIp, port: nodeAPort, neighbors: new List<IPEndPoint>
                    {
                        // Neighbors of first node
                        new IPEndPoint(IPAddress.Parse(nodeBIp), nodeBPort),
                        new IPEndPoint(IPAddress.Parse(nodeCIp), nodeCPort),
                    }
                ),
                new NodeCreationInfo(name: "B", size: 7, ip: nodeBIp, port: nodeBPort, neighbors: new List<IPEndPoint>
                    {
                        // Neighbors of second node
                        new IPEndPoint(IPAddress.Parse(nodeAIp), nodeAPort),
                        new IPEndPoint(IPAddress.Parse(nodeDIp), nodeCPort),
                    }
                ),
                new NodeCreationInfo(name: "C", size: 11, ip: nodeCIp, port: nodeCPort, neighbors: new List<IPEndPoint>
                    {
                        // Neighbors of third node
                        new IPEndPoint(IPAddress.Parse(nodeAIp), nodeAPort),
                        new IPEndPoint(IPAddress.Parse(nodeBIp), nodeBPort),
                    }
                ),
                new NodeCreationInfo(name: "D", size: 23, ip: nodeDIp, port: nodeDPort, neighbors: new List<IPEndPoint>
                    {
                        // Neighbors of third node
                        new IPEndPoint(IPAddress.Parse(nodeBIp), nodeAPort),
                        new IPEndPoint(IPAddress.Parse(nodeCIp), nodeBPort),
                    }
                )
            };

            Printer.Print($"Starte UDP Client auf Port {configurationPort}... ");
            try
            {
                // Create UdpClient
                udpClient = new UdpClient(configurationPort);
            }
            catch (Exception e)
            {
                Printer.PrintLine("FAIL", ConsoleColor.Red);
                Printer.PrintLine($"Exception caught while trying to create UDP Client: {e.Message}");
            }
            
            Printer.PrintLine("OK", ConsoleColor.Green);

            // Send the initialization info to each point
            foreach (var nodeToBeCreated in nodeCreationInfos)
            {
                IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Any, 0);

                Printer.Print("Waiting for incoming package with port... ");

                // Wait for incoming message on configuration port
                byte[] incomingBytes = udpClient.Receive(ref ipEndpoint);
                
                Printer.PrintLine("OK", ConsoleColor.Green);

                // Convert received bytes to int
                int receivedPort = BitConverter.ToInt32(incomingBytes, 0);

                Printer.PrintLine($"Empfanger Port: {receivedPort}");
                Printer.Print($"Sende Knoten Informationen an {nodeToBeCreated.Ip}:{receivedPort}... ");

                // Send the info to the current point
                SendCreationInfoToNode(IPAddress.Parse(nodeToBeCreated.Ip), receivedPort, nodeToBeCreated);

                Printer.PrintLine("OK", ConsoleColor.Green);
            }

            Printer.PrintLine("Konfiguration abgeschlossen. Starte Logger... ");

            try
            {
                // When finished start the logger process
                Process loggerProcess = new Process { StartInfo = { FileName = "..\\..\\..\\EchoVs_logger\\bin\\Debug\\EchoVS3_Logger.exe" } };
                loggerProcess.Start();
            }
            catch (Exception e)
            {
                Printer.PrintLine("FAIL", ConsoleColor.Red);
                Printer.PrintLine($"Exception caught while starting logger process: {e.Message}");
            }

            Printer.PrintLine("OK", ConsoleColor.Green);

            Printer.PrintLine("Beliebige Taste zum Schließen drücken...");
            Console.ReadKey();
        }

        private static void SendCreationInfoToNode(IPAddress ipAddress, int configurationPort, NodeCreationInfo nodeCreationInfo)
        {
            UdpClient udpClient = new UdpClient();
            byte[] formattedNodeCreationInfo;

            // Convert NodeCreationInfo to byte[]
            using (var ms = new MemoryStream())
            {
                try
                {
                    // Serialize the object
                    (new BinaryFormatter()).Serialize(ms, nodeCreationInfo);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: Error when parsing to byte array: {e.Message}");
                    throw;
                }

                formattedNodeCreationInfo = ms.ToArray();
            }

            // Send the info to the client
            udpClient.Send(formattedNodeCreationInfo, formattedNodeCreationInfo.Length, new IPEndPoint(ipAddress, configurationPort));
        }
    }
}
