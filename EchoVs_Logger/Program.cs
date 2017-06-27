using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EchoVS3;
using Type = EchoVS3.Type;

namespace EchoVS3_Logger
{
    class Program
    {
        static void Main()
        {
            const int loggerPort = 1234;
            bool continueListening = true;
            string input = "";
            bool printOnLateFinish = false;
            uint sequenceNumber = 0;

            // Start listening for messages
            UdpClient udpClient = new UdpClient(loggerPort);
            udpClient.Client.ReceiveTimeout = 10000;
            IPEndPoint senderIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint masterNode = null;

            // Set console colors
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();

            Printer.Print("Starte Logger... ");

            Task udpListenerTask = Task.Run(() =>
            {
                while (continueListening)
                {
                    byte[] incomingByteArray;

                    // Receive message
                    try
                    {
                        incomingByteArray = udpClient.Receive(ref senderIpEndPoint);
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode == SocketError.TimedOut)
                        {
                            // If set to keep listening continue
                            if(continueListening)
                                continue;

                            // Else stop
                            return;
                        }

                        Printer.PrintLine($"Exception caught in udpListenerTask: {e.Message}");
                        throw;
                    }

                    // Convert byte array to message
                    Message incomingMessage = Message.FromByteArray(incomingByteArray);

                    // Print message
                    if (incomingMessage.Type == Type.Logging)
                    {
                        Printer.PrintLine(
                            $"{DateTime.Now:T}: Von: {senderIpEndPoint.Address}:{senderIpEndPoint.Port} | {incomingMessage.Data}");
                    }
                    else if (incomingMessage.Type == Type.Echo)
                    {
                        Printer.PrintLine(
                            $"{DateTime.Now:T}: Echo-Algorithmus (Seq: {incomingMessage.Number}) abgeschlossen. Data: {incomingMessage.Data}", ConsoleColor.Green);
                    }
                }
            });

            Printer.PrintLine("OK", ConsoleColor.Green);
            Printer.Print("Starte Logger observer... ");

            // Start task that will print out that the listener has stopped if the timeout is already over
            Task callbackAfterFinishedTask = udpListenerTask.ContinueWith(listenerResult =>
            {
                if (printOnLateFinish)
                    Printer.PrintLine("Listener beendet. Programm kann nun beendet werden", ConsoleColor.Blue);
            });

            Printer.PrintLine("OK", ConsoleColor.Green);
            Printer.PrintLine("Folgende Kommandos möglich:");
            Printer.PrintLine("STOP/EXIT/QUIT - Logger beenden");
            Printer.PrintLine("START - Echoanstoßmodus starten");

            while (true)
            {
                // Wait for keyboard input
                if (Console.KeyAvailable)
                {
                    input = Console.ReadLine();
                }
                else
                {
                    Thread.Sleep(200);
                }

                if (input == "STOP" || input == "QUIT" || input == "EXIT" || input == "STAHP")
                {
                    // Stop the task
                    Printer.Print("Beende Listener... ");

                    // Set the flag to stop
                    continueListening = false;

                    // Wait for the task to finish
                    if (udpListenerTask.Wait(TimeSpan.FromSeconds(5)))
                    {                        
                        Printer.PrintLine("OK", ConsoleColor.Green);
                        Printer.PrintLine("Beenden mit beliebiger Taste...");
                    }
                    else
                    {
                        Printer.PrintLine("FAIL", ConsoleColor.Red);
                        Printer.PrintLine("Listener konnte nicht beendet werden. Mit beliebiger Taste trotzdem beenden...");

                        // Set variable to print out when listener finishes
                        printOnLateFinish = true;
                    }
                    
                    // Wait for keypress
                    Console.ReadKey();

                    return;
                }

                if (input == "START")
                {
                    try
                    {
                        if (masterNode == null)
                        {
                            Printer.PrintLine("Noch kein Masterknoten gesetzt.");

                            // Ask for ip and port
                            Printer.Print("Bitte IP für Masterknoten angeben: ");
                            IPAddress ipAddress = IPAddress.Parse(Console.ReadLine());

                            Printer.Print("Bitte Port für Masterknoten angeben");
                            int port = int.Parse(Console.ReadLine());

                            // Create ipEndPoint for master node
                            masterNode = new IPEndPoint(ipAddress, port);
                        }
                        else
                        {
                            Printer.PrintLine($"Masterknoten bereits bekannt: {masterNode.Address}:{masterNode.Port}");
                        }

                        Printer.Print("Echo-Algorithmus starten? J/N ");

                        input = Console.ReadLine();

                        if(input != "J")
                            continue;

                        Printer.PrintLine($"Sende Startnachricht an Masterknoten (Seq: {sequenceNumber})... ");

                        // Create starting message
                        Message startingMessage = new Message(Type.Info, sequenceNumber, "ECHO_START");

                        // Convert to byte array
                        byte[] messageBytes = startingMessage.ToByteArray();

                        // Send message to masternode
                        try
                        {
                            udpClient.Send(messageBytes, messageBytes.Length, masterNode);
                        }
                        catch (Exception e)
                        {
                            Printer.PrintLine("FAIL", ConsoleColor.Red);
                            Printer.PrintLine($"Exception caught while sending message to master node: {e.Message}", ConsoleColor.Red);
                            continue;
                        }
                        
                        Printer.PrintLine("OK", ConsoleColor.Green);
                        Printer.PrintLine("Log:");
                    }
                    catch (Exception e)
                    {
                        Printer.PrintLine($"Exception caught while inputting information for master node: {e.Message}", ConsoleColor.Red);
                        continue;
                    }
                }
            }
        }
    }
}
