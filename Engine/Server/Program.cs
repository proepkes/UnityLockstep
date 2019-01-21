using System;
using System.Diagnostics;
using System.Threading;      

namespace Server
{
    class Program
    {                                   
        static void Main(string[] args)
        {
            int roomSize = 2;
            int waitInSeconds = 5;

            var sw = new Stopwatch();
            sw.Start();
            Console.Write($"Enter room size (defaults to {roomSize} after {waitInSeconds} seconds): ");
            while (!Console.KeyAvailable && sw.Elapsed.Seconds < waitInSeconds)
            {                  
                Thread.Sleep(1);
            }
            sw.Stop();

            if (Console.KeyAvailable)
            {
                roomSize = Console.ReadKey().KeyChar - 48;             
            }
            Console.Write(Environment.NewLine);

            var room = new Room();

            room.Open(roomSize);         

            while (!Console.KeyAvailable)
            {
                room.Update();
                Thread.Sleep(1);
            }

            room.Close();
        }       
    }
}
