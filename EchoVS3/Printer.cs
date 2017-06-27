﻿using System;

namespace EchoVS3
{
    public static class Printer
    {
        private static object _lock = new object();

        /// <summary>
        /// Prints a single line in the currently set color
        /// </summary>
        /// <param name="message">The message to print</param>
        public static void Print(string message)
        {
            lock(_lock)
                Console.Write(message);
        }

        /// <summary>
        /// Prints a message in the given color
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <param name="color">The color for the message</param>
        public static void Print(string message, ConsoleColor color)
        {
            lock (_lock)
            {
                ConsoleColor temp = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Write(message);
                Console.ForegroundColor = temp;
            }
        }

        /// <summary>
        /// Prints a message in the currently selected color and ends with a new line
        /// </summary>
        /// <param name="message">The message to print</param>
        public static void PrintLine(string message)
        {
            lock(_lock)
                Console.WriteLine(message);
        }

        /// <summary>
        /// Prints a message in the given color and ends with a new line
        /// </summary>
        /// <param name="message">The message to print</param>
        /// <param name="color">The color for the message</param>
        public static void PrintLine(string message, ConsoleColor color)
        {
            lock (_lock)
            {
                ConsoleColor temp = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor = temp;
            }
        }
    }
}
