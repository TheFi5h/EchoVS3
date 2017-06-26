using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using EchoVS3;
using System.Threading;
using System.Threading.Tasks;

namespace EchoVS3_Node
{
    class Program
    {
        private static Node _node;
        private static int configurationPort = 9999;

        private static byte[] _receivedBytes;
        private static bool _messageReceived;

        static void Main()
        {
            // Start to listen on the configuration port for configuration messages
            var ipEndPoint = new IPEndPoint(IPAddress.Any, configurationPort);
            var udpClient = new UdpClient(ipEndPoint);
            string input;
            bool printOnLateFinish = false;

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();

            Printer.Print($"Beginne Empfang von Netzwerkerstellungsnachrichten auf Port {configurationPort}... ");

            // Start listening to the configuration port
            udpClient.BeginReceive(ReceiveCallback, new UdpState(udpClient, ipEndPoint));

            Printer.PrintLine("OK", ConsoleColor.Green);

            Printer.PrintLine("Für manuelle Erstellung des Knotens beliebige Taste drücken");

            // Start listening for keyboard or network input
            do
            {
                // Will wait for either a message received or a key pressed
                Thread.Sleep(100);

            } while (_messageReceived != true && Console.KeyAvailable != true);

            // Check what has been triggered
            if (_messageReceived)
            {
                // Convert received bytes to an node creation info object
                var nodeCreationInfo = NodeCreationInfo.FromByteArray(_receivedBytes);

                // Create the node with the given info
                _node = new Node(nodeCreationInfo);
            }
            else if (Console.KeyAvailable)
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
                Printer.Print("Bitte eigene IP angeben: ");
                nodeCreationInfo.Ip = Console.ReadLine();

                Printer.Print("Bitte Port angeben: ");
                nodeCreationInfo.Port = int.Parse(Console.ReadLine());

                Printer.Print("Bitte einen Namen für den Knoten eingeben: ");
                nodeCreationInfo.Name = Console.ReadLine();

                Printer.Print("Bitte die Größe des Knotens angeben: ");
                nodeCreationInfo.Size = uint.Parse(Console.ReadLine());

                // Get n-times neighbor information
                while (true)
                {
                    // Get ip for neighbor
                    Printer.Print("Bitte IP für Nachbar angeben (Hinzufügen beenden mit Enter ohne Eingabe): ");
                    input = Console.ReadLine();

                    // Check if enter pressed without input
                    if (input == string.Empty)
                        break;

                    // Get port of neighbor
                    Printer.Print("Bitte Port für Nachbar angeben: ");
                    var neighborEndPoint = new IPEndPoint(IPAddress.Parse(input), int.Parse(Console.ReadLine()));

                    // Add the endpoint to the list
                    nodeCreationInfo.Neighbors.Add(neighborEndPoint);
                }

                Printer.Print($"Erstelle Knoten mit {nodeCreationInfo.Neighbors.Count} Nachbarn... ");

                try
                {
                    _node = new Node(nodeCreationInfo);
                }
                catch (Exception e)
                {
                    Printer.PrintLine("FAIL", ConsoleColor.Red); 
                    Printer.PrintLine($"Exception message: {e.Message}");
                    Printer.PrintLine("Beliebige Taste zum Verlassen drücken...");

                    // Wait for keypress
                    Console.ReadKey();
                    return;
                }

                Printer.PrintLine("OK", ConsoleColor.Green);
            }

            Printer.PrintLine("Starte Knoten. STOP/EXIT/QUIT eingeben zum Beenden!");
            Printer.PrintLine("-----------------------------------------------------------");

            // Run node in task
            Task nodeTask = Task.Run(() =>
            {
                try
                {
                    _node.Run();
                }
                catch (Exception e)
                {
                    Printer.PrintLine($"Exception caught in Program while executing node: {e.Message}", ConsoleColor.Red);
                    throw;
                }
            });

            Task nodeTaskWaiter = nodeTask.ContinueWith(nodeTaskResult =>
            {
                if(printOnLateFinish)
                    Printer.PrintLine("Knoten beendet. Programm kann nun beendet werden.", ConsoleColor.Yellow);
            });

            // Clear input
            input = "";

            // Listen for keyboard input
            do
            {
                if (Console.KeyAvailable)
                {
                    input = Console.ReadLine();
                }
                else
                {
                    Thread.Sleep(200);
                }
            } while (input != "STOP" && input != "EXIT" && input != "QUIT" && input != "STAHP");

            Printer.PrintLine("Beende Knoten...");

            // Stop the node
            _node.Stop();

            Printer.Print("Warte auf Knoten... ");

            // Wait 10 seconds for task to finish
            if (nodeTask.Wait(TimeSpan.FromSeconds(5)))
            {                
                Printer.PrintLine("OK", ConsoleColor.Green);
                Printer.PrintLine("Beliebige Taste zum Verlassen drücken...");
            }
            else
            {
                Printer.PrintLine("FAIL", ConsoleColor.Red);
                Printer.PrintLine("Task wurde noch nicht beendet. Mit beliebiger Taste trotzdem beenden.");

                // Set flag to let task know to print out when the node task finished
                printOnLateFinish = true;
            }

            Console.ReadKey();
        }

        private static void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                // Get the udp client saved in the udp state
                var udpClient = ((UdpState)result.AsyncState).UdpClient;
                var ipEndPoint = ((UdpState)result.AsyncState).IPEndPoint;

                // Check what has been received and end receiving
                _receivedBytes = udpClient.EndReceive(result, ref ipEndPoint);
                _messageReceived = true;
            }
            catch (Exception)
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
