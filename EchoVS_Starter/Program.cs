using EchoVS3;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EchoVS_Starter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            Printer.PrintLine("Dieses Programm startet automatisch alle benötigten Prozesse. Nach der Erstellung der Prozesse können " +
                              "diese auch hier wieder geschlossen werden.\n");

            List<Process> processes = new List<Process>();

            Printer.Print("Starte Logger... ");
            Process loggerProcess = new Process { StartInfo = { FileName = "..\\..\\..\\EchoVs_Logger\\bin\\Debug\\EchoVS3_Logger.exe" } };
            loggerProcess.Start();

            processes.Add(loggerProcess);
            
            Printer.PrintLine("OK", ConsoleColor.Green);

            Printer.Print("Starte Knotenersteller... ");
            Process nodeNetworkCreator = new Process { StartInfo = { FileName = "..\\..\\..\\EchoVS3_NodeNetworkCreator\\bin\\Debug\\EchoVS3_NodeNetworkCreator.exe" } };
            nodeNetworkCreator.Start();

            processes.Add(nodeNetworkCreator);
            
            Printer.PrintLine("OK", ConsoleColor.Green);
            Printer.PrintLine("Bitte zuerst NodeNetworkCreator konfigurieren!", ConsoleColor.Yellow);
            Printer.PrintLine("Weiter mit beliebiger Taste... ");
            Console.ReadKey();

            Printer.Print("Bitte Anzahl der zu erstellenden Knoten angeben: ");
            int numberOfNodes = int.Parse(Console.ReadLine());

            Printer.PrintLine("Starte Knoten...");

            for (int i = 0; i < numberOfNodes; i++)
            {
                Console.SetCursorPosition(0, Console.CursorTop);

                Printer.Print($"{i+1}/{numberOfNodes}");

                Process node = new Process { StartInfo = { FileName = "..\\..\\..\\EchoVS3_Node\\bin\\Debug\\EchoVS3_Node.exe" } };
                node.Start();

                processes.Add(node);
            }

            Printer.PrintLine("\nErstellung abgeschlossen", ConsoleColor.Green);
            Printer.PrintLine("Beliebige Taste drücken um die Prozesse zu schließen... ");

            Console.ReadKey();

            Printer.Print("Schließe Prozesse... ");

            foreach (var p in processes)
            {
                try
                {
                    // Kill the process
                    p.Kill();
                }
                catch (Exception)
                {
                    // Catch exception if process has already been killed
                }
            }

            Printer.PrintLine("OK", ConsoleColor.Green);

            Printer.PrintLine("Programm beenden mit beliebiger Taste... ");
            Console.ReadKey();
        }
    }
}
