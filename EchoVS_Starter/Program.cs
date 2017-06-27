using EchoVS3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EchoVS_Starter
{
    class Program
    {
        private static int sleepTime = 100;
        static void Main(string[] args)
        {
            Printer.Print("Starting node network creator...");
            Process nodeNetworkCreator = new Process { StartInfo = { FileName = "..\\..\\..\\EchoVS3_NodeNetworkCreator\\bin\\Debug\\EchoVS3_NodeNetworkCreator.exe" } };
            nodeNetworkCreator.Start();
            Printer.PrintLine("OK", ConsoleColor.Green);
            Thread.Sleep(sleepTime);

            Printer.Print("Starting nodes...");
            Process node1 = new Process { StartInfo = { FileName = "..\\..\\..\\EchoVS3_Node\\bin\\Debug\\EchoVS3_Node.exe" } };
            node1.Start();
            Printer.PrintLine("1 OK", ConsoleColor.Green);
            Thread.Sleep(sleepTime);
            Process node2 = new Process { StartInfo = { FileName = "..\\..\\..\\EchoVS3_Node\\bin\\Debug\\EchoVS3_Node.exe" } };
            node2.Start();
            Printer.PrintLine("2 OK", ConsoleColor.Green);
            Thread.Sleep(sleepTime);
            Process node3 = new Process { StartInfo = { FileName = "..\\..\\..\\EchoVS3_Node\\bin\\Debug\\EchoVS3_Node.exe" } };
            node3.Start();
            Printer.PrintLine("3 OK", ConsoleColor.Green);
            Thread.Sleep(sleepTime);
            Process node4 = new Process { StartInfo = { FileName = "..\\..\\..\\EchoVS3_Node\\bin\\Debug\\EchoVS3_Node.exe" } };
            node4.Start();
            Printer.PrintLine("4 OK", ConsoleColor.Green);
        }
    }
}
