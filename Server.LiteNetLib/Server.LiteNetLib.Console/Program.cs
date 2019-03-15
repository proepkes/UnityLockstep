using System;
using System.Diagnostics;
using System.Threading;
using Lockstep.Network.Server;
using Server.LiteNetLib;

namespace Server
{
    class Program
    {                                   
        static void Main(string[] args)
        {
            int roomSize = 2;            

            var server = new LiteNetLibServer();

            var room = new Room(server, roomSize);

            room.Open(9050);         

            while (true)
            {
                server.PollEvents();
                Thread.Sleep(1);
            }
                          
        }       
    }
}
