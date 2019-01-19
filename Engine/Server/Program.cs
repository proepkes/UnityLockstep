using System;
using System.Threading;      

namespace Server
{
    class Program
    {
        private const int RoomSize = 1;

        static void Main(string[] args)
        {                    
            var room = new Room();

            room.Open(RoomSize);         

            while (!Console.KeyAvailable)
            {
                room.Update();
                Thread.Sleep(1);
            }

            room.Close();
        }       
    }
}
