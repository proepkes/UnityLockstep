using System;
using System.Threading;      

namespace Server
{
    class Program
    {         
        static void Main(string[] args)
        {                    
            var room = new Room();                              

            room.Open(2);         

            while (!Console.KeyAvailable)
            {
                room.Update();
                Thread.Sleep(1);
            }

            room.Close();
        }       
    }
}
