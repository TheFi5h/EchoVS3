using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoVS3
{
    public class NodeNetworkGenerator
    {
        public List<int> Nodes { get; }

        private readonly Random _random;
        private readonly int _maxConnections;

        public NodeNetworkGenerator(int seed, int maxConnections = 3)
        {
            Nodes = new List<int>();
            // Create a new random with the given seed
            _random = new Random(seed);
            _maxConnections = maxConnections;
        }

        /// <summary>
        /// Generates a network with the given nodes. There must be a minimum of 2 nodes to generate a network.
        /// Networks with same nodes and same random seed will be equal but only on first generation.
        /// </summary>
        /// <returns>A list of the nodes (keys) and its connections entries of list (values)</returns>
        public Dictionary<int, List<int>> Generate()
        {
            if (Nodes.Count < 2)
                throw new InvalidOperationException("Cannot build a network with less than 2 nodes");

            Dictionary<int, List<int>> nodeConnections = new Dictionary<int, List<int>>();

            // Create a first network so that each node has min 1 connection to another
            CreatePrimaryConnections(nodeConnections);

            // Add random connections until the average connections of all nodes equal half of the maximum connections
            while (nodeConnections.Average(c => c.Value.Count) - 1 <= (double)(_maxConnections - 1)/2)
            {
                // Get two random nodes
                int firstNode = _random.Next(nodeConnections.Count);
                int secondNode = firstNode;

                // Search for a second node which is not the first node
                while (secondNode == firstNode)
                    secondNode = _random.Next(nodeConnections.Count);

                // Check if one of the two nodes has already reached the maximum connections
                if (nodeConnections[firstNode].Count == _maxConnections ||
                    nodeConnections[secondNode].Count == _maxConnections)
                {
                    continue;
                }

                // Create a connection between the two nodes
                nodeConnections[firstNode].Add(secondNode);
                nodeConnections[secondNode].Add(firstNode);
            }

            // Distinctify lists to ensure no duplicate entries
            Dictionary<int, List<int>> returnDictionary = new Dictionary<int, List<int>>(nodeConnections.Count);

            foreach (var nodeConnection in nodeConnections)
            {
                returnDictionary.Add(nodeConnection.Key, nodeConnection.Value.Distinct().ToList());
            }


            // Return the finished dictionary with connections
            return returnDictionary;
        }

        private void CreatePrimaryConnections(Dictionary<int, List<int>> nodeConnections)
        {
            // Create copy of list of nodes
            List<int> nodes = new List<int>(Nodes);

            // Get starting node
            int randomNumber = _random.Next(nodes.Count);
            int currentNode = nodes.ElementAt(randomNumber);
            nodes.RemoveAt(randomNumber);
            nodeConnections.Add(currentNode, new List<int>(_maxConnections));

            while (nodes.Any())
            {
                // Get the node to connect
                randomNumber = _random.Next(nodes.Count);
                int nextNode = nodes.ElementAt(randomNumber);
                nodes.RemoveAt(randomNumber);

                // Add a connection list to the next node
                nodeConnections.Add(nextNode, new List<int>(_maxConnections));

                // Add the connection to each other to the connection lists
                nodeConnections[currentNode].Add(nextNode);
                nodeConnections[nextNode].Add(currentNode);

                currentNode = nextNode;
            }
        }
    }
}
