using EchoVS3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace EchoVS3_NodeNetworkCreator
{
    class Program
    {
        static void Main()
        {
            const int configurationPort = 9999;
            const string nodeIp = "192.168.0.105";

            UdpClient udpClient;

            // Edit console design
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            
            
            int numberOfNodes;
            do
            {
                Printer.Print("Anzahl der zu erstellenden Knoten angeben (Minimum: 2 | Standard: 5): ");
                int.TryParse(Console.ReadLine(), out numberOfNodes);

                // If nothing entered, set to default
                if (numberOfNodes == 0)
                    numberOfNodes = 5;

            } while (numberOfNodes < 2);

            int numberOfMaxConnections = 3;
            do
            {
                Printer.Print("Anzahl der maximalen Verbindungen angeben (Minimum: 2 | Standard: 3): ");
                int.TryParse(Console.ReadLine(), out numberOfMaxConnections);

                // If nothing entered, set to default
                if (numberOfMaxConnections == 0)
                    numberOfMaxConnections = 3;

            } while (numberOfMaxConnections < 2);

                Printer.Print("Bitte eine zufällige Zahl eingeben (Für gleiche Zufallswerte sieht das Netzwerk gleich aus): ");
            int randomSeed = int.Parse(Console.ReadLine());

            // Create random
            Random random = new Random(randomSeed);

            List<NodeCreationInfo> nodeCreationInfos = new List<NodeCreationInfo>();
            List<int> usedPorts = new List<int>(numberOfNodes);

            Printer.Print("Suche zufällige Ports für Endpunkte... ");

            // Create nodes
            for (int i = 0; i < numberOfNodes; i++)
            {
                int port;

                // Get random port between 1024 and unsigned short max value
                do
                {
                    // Min 1235 because logger is 1234
                    port = random.Next(1235, ushort.MaxValue);
                } while (usedPorts.Contains(port));

                // Add the port to the used ports
                usedPorts.Add(port);

                // Create a new node without neighbors
                nodeCreationInfos.Add(new NodeCreationInfo($"{i.GetHashCode():X}", (uint)i + 1, nodeIp, port, new List<IPEndPoint>(numberOfMaxConnections)));
            }

            Printer.PrintLine("OK", ConsoleColor.Green);

            NodeNetworkGenerator nodeNetworkGenerator = new NodeNetworkGenerator(15);

            // Add the indexes of the nodes to the NodeNetworkGenerator
            nodeNetworkGenerator.Nodes.AddRange(Enumerable.Range(0, nodeCreationInfos.Count));

            Printer.Print("Generiere Knotennetzwerk... ");
            try
            {
                // Let the generator generate the connections
                var nodeConnections = nodeNetworkGenerator.Generate();

                // Translate the made connections to the nodeCreationInfos
                foreach (var element in nodeConnections)
                {
                    // Get the references connection endpoints
                    List<IPEndPoint> endpoints = new List<IPEndPoint>(numberOfMaxConnections);

                    foreach (var connectionIndex in element.Value)
                    {
                        endpoints.Add(new IPEndPoint(IPAddress.Parse(nodeCreationInfos.ElementAt(connectionIndex).Ip), nodeCreationInfos.ElementAt(connectionIndex).Port));
                    }

                    // Add the endpoints as neighbors
                    nodeCreationInfos.ElementAt(element.Key).Neighbors.AddRange(new List<IPEndPoint>(endpoints));
                }
            }
            catch (Exception e)
            {
                Printer.PrintLine("FAIL", ConsoleColor.Red);
                Printer.PrintLine($"Exception caught while generating network: {e.Message}");
                Printer.PrintLine("Programm mit beliebiger Taste beenden... ");
                Console.ReadKey();
                return;
            }
            
            Printer.PrintLine("OK", ConsoleColor.Green);

            Printer.PrintLine("Netzwerk:");

            foreach (var node in nodeCreationInfos)
            {
                Printer.Print($"'{node.Name}' {node.Ip}:{node.Port}: Nachbarn (Ports): ");
                foreach (var neighborNode in node.Neighbors)
                {
                    Printer.Print($"{neighborNode.Port} ");
                }
                Printer.PrintLine("");
            }

            Printer.Print($"\nStarte UDP Client auf Port {configurationPort}... ");
            try
            {
                // Create UdpClient
                udpClient = new UdpClient(configurationPort);
            }
            catch (Exception e)
            {
                Printer.PrintLine("FAIL", ConsoleColor.Red);
                Printer.PrintLine($"Exception caught while trying to create UDP Client: {e.Message}");
                Printer.PrintLine("Programm mit beliebiger Taste beenden... ");
                Console.ReadKey();
                return;
            }
            
            Printer.PrintLine("OK", ConsoleColor.Green);
            int nodesCreated = 0;

            // Send the initialization info to each point
            foreach (var nodeToBeCreated in nodeCreationInfos)
            {
                IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Any, 0);

                Printer.Print($"Waiting for incoming package ({nodesCreated + 1}/{nodeCreationInfos.Count})... ");

                // Wait for incoming message on configuration port
                byte[] incomingBytes = udpClient.Receive(ref ipEndpoint);

                Printer.PrintLine("PACKAGE RECEIVED", ConsoleColor.Green);

                // Convert received bytes to int
                int receivedPort = BitConverter.ToInt32(incomingBytes, 0);

                Printer.PrintLine($"Empfanger Port: {receivedPort}");
                Printer.Print($"Sende Knoten Informationen an {nodeToBeCreated.Ip}:{receivedPort}... ");

                // Send the info to the current point
                SendCreationInfoToNode(IPAddress.Parse(nodeToBeCreated.Ip), receivedPort, nodeToBeCreated);

                Printer.PrintLine("OK", ConsoleColor.Green);

                nodesCreated++;
            }

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
