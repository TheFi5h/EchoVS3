using EchoVS3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace EchoVS3_NodeNetworkCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            const int configurationPort = 9999;

            string nodeAIp = "192.168.178.10";
            string nodeBIp = "192.168.178.110";
            string nodeCIp = "192.168.178.220";

            int nodeAPort = 1111;
            int nodeBPort = 2222;
            int nodeCPort = 3333;

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
                        new IPEndPoint(IPAddress.Parse(nodeCIp), nodeCPort),
                    }
                ),
                new NodeCreationInfo(name: "C", size: 11, ip: nodeCIp, port: nodeCPort, neighbors: new List<IPEndPoint>
                    {
                        // Neighbors of third node
                        new IPEndPoint(IPAddress.Parse(nodeAIp), nodeAPort),
                        new IPEndPoint(IPAddress.Parse(nodeBIp), nodeBPort),
                    }
                )
            };

            // Send the initialization info to each point
            foreach(var nodeToBeCreated in nodeCreationInfos)
            {
                // Send the info to the current point
                SendCreationInfoToNode(IPAddress.Parse(nodeToBeCreated.Ip), configurationPort, nodeToBeCreated);
            }
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
