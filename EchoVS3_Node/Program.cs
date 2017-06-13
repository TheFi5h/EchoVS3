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

namespace EchoVS3_Node
{
    class Program
    {
        private static Node _node;
        private static UdpClient _nodeUdpClient;
        private static UdpClient _loggerUdpClient;
        private static BinaryFormatter _binaryFormatter = new BinaryFormatter();

        private const bool _debug = true;

        static void Main(string[] args)
        {
            _node = ExtractParametersAndCreateNode(args);

            if (_node == null)
            {
                Console.WriteLine("Error: Node could not be created");
                return;
            }

            
        }

        private static Node ExtractParametersAndCreateNode(string[] args)
        {
            // Set parametersLeft (Length - Path - Count of static parameters (name, size, ip, port, neighbor_ip, neighbor_port))
            int parametersLeft = args.Length - 7;

            // Extract parameters from args
            // Parameters will be: name size ip port (ip port)+
            if (args.Length < 7)
                Console.WriteLine($"Error: Only {args.Length} parameters given. Minimum is 7 (First beeing the path)");

            // 1 = name (string)
            string name = args[1];

            // 2 = size (uint)
            if (!uint.TryParse(args[2], out uint size))
                Console.WriteLine("Error: Third parameter (size) wasn't parsable. Must be unsigned int");

            // 3 = ip (string)
            string ip = args[3];

            // 4 = port (uint)
            if (!int.TryParse(args[4], out int port))
                Console.WriteLine("Error: Fifth parameter (port) wasn't parsable. Must be int");

            IPEndPoint nodeEndPoint = null;
            try
            {
                nodeEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: Creating the IPEndpoint failed: {e.Message}");
            }

            List<IPEndPoint> neighbors = new List<IPEndPoint>();
            int index = 5;

            // Keep extracting neighbors as long as possible
            while (parametersLeft >= 2)
            {
                if (!int.TryParse(args[4], out int neighborPort))
                    Console.WriteLine($"Error: Parameter {index} (neighborPort) wasn't parsable. Must be int");

                // Add the endpoint to the list
                neighbors.Add(new IPEndPoint(IPAddress.Parse(args[index]), neighborPort));

                parametersLeft -= 2;
            }

            // Check if endpoint could be created
            if (nodeEndPoint != null)
            {
                // Create node
                Node node = new Node(name, size, nodeEndPoint);

                // Add neighbors to node
                node.NeighborEndPoints.AddRange(neighbors);

                return node;
            }
            else
                return null;
        }
    }
}
