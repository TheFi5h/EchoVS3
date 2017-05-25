using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EchoVS3;

namespace EchoVS3_Node
{
    class Program
    {
        private static Node _node;
        private static UdpClient _udpClient;

        static void Main(string[] args)
        {
            #region Extract Parameters
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

            System.Net.IPEndPoint nodeEndPoint;
            try
            {
                nodeEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: Creating the IPEndpoint failed: {e.Message}");
                return;
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

            #endregion

            // Crate node
            _node = new Node(name, size, nodeEndPoint);

            // Add neighbors to node
            _node.NeighborEndPoints.AddRange(neighbors);
            
            // Create UDP Sockets
            _udpClient = new UdpClient(_node.NodeEndPoint);
            
        }
    }
}
