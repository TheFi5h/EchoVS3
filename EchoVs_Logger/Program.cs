using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EchoVS3;

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

            // Start listening for messages
            UdpClient udpClient = new UdpClient(loggerPort);
            udpClient.Client.ReceiveTimeout = 10000;
            IPEndPoint senderIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.ForegroundColor = ConsoleColor.White;

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
                    Printer.PrintLine($"{DateTime.Now:T}: From: {senderIpEndPoint.Address}:{senderIpEndPoint.Port} | {incomingMessage.Data}");
                }
            });

            // Start task that will print out that the listener has stopped if the timeout is already over
            Task callbackAfterFinishedTask = udpListenerTask.ContinueWith(listenerResult =>
            {
                if (printOnLateFinish)
                    Printer.PrintLine("Listener beendet. Programm kann nun beendet werden", ConsoleColor.Yellow);
            });

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
                
            }

            
        }
    }
}
