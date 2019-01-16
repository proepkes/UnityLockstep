using System;
using System.Collections.Generic;
using System.Threading;
using ECS.Data;
using LiteNetLib;
using LiteNetLib.Utils;   
using Lockstep.Framework.Networking;
using Lockstep.Framework.Networking.Serialization;

namespace Server
{
    public class Room
    {
        public bool Running { get; private set; }    

        private const int TargetFps = 1;
        private const int ServerPort = 9050;       
        private const string ClientKey = "SomeConnectionKey";


        private FramePacker _framePacker;

        private readonly NetManager _server;
        private readonly EventBasedNetListener _listener;
        private readonly Dictionary<int, byte> _peerIds = new Dictionary<int, byte>();
        private readonly Dictionary<ulong, ulong> _checksums = new Dictionary<ulong, ulong>();

        public Room()
        {
            _listener = new EventBasedNetListener();
            _server = new NetManager(_listener);

        }            

        public void Distribute(NetDataWriter data)
        {                                        
            foreach (var peer in _server.ConnectedPeerList)
            {
                peer.Send(data, DeliveryMethod.ReliableOrdered);
            }
        }

        public void Open(int roomSize)
        {                    
           Console.WriteLine("Starting server for " + roomSize + " players...");
            _server.Start(ServerPort);      

            _listener.ConnectionRequestEvent += request =>
            {
                if (_server.PeersCount < roomSize)
                {
                    request.AcceptIfKey(ClientKey);
                }
                else
                {
                    request.Reject();
                }
            };

            _listener.PeerConnectedEvent += peer =>
            {         
                if (_server.PeersCount == roomSize)
                {
                    Console.WriteLine("Room is full, starting new simulation...");
                    new Thread(Loop) { IsBackground = true }.Start(); 
                }          
            };

            _listener.NetworkReceiveEvent += (peer, reader, method) =>
            {
                var messageTag = (MessageTag)reader.GetByte();
                switch (messageTag)
                {
                    case MessageTag.Command:  
                        _framePacker?.AddCommand(new Command { Data = reader.GetRemainingBytes() });
                        break;
                    case MessageTag.Checksum:
                        var pkt = new Checksum();
                        pkt.Deserialize(reader);
                        if (!_checksums.ContainsKey(pkt.FrameNumber))
                        {
                            _checksums[pkt.FrameNumber] = pkt.Value;
                        }
                        else
                        {
                            Console.WriteLine((_checksums[pkt.FrameNumber] == pkt.Value ? "Checksum valid" : "Desync") +": " + pkt.Value); 
                        }
                        break;
                }
            };

            _listener.PeerDisconnectedEvent += (peer, info) =>
            {
                if (_server.ConnectedPeerList.Count == 0)
                {
                    Console.WriteLine("All players left, stopping current simulation...");
                    Running = false;
                }
            };
        }

        public void Update()
        {      
            _server.PollEvents();
        }

        public void Close()
        {
            _server.Stop();
        }                 

        private void Loop()
        {
            var timer = new Timer();
            var dt = 1000.0 / TargetFps;

            var accumulatedTime = 0.0;

            Running = true;    

            var writer = new NetDataWriter();

            byte playerId = 0;
            var seed = new Random().Next(int.MinValue, int.MaxValue);
            foreach (var peer in _server.ConnectedPeerList)
            {
                _peerIds[peer.Id] = playerId++;

                writer.Reset();
                writer.Put((byte)MessageTag.Init);
                new Init { Seed = seed, TargetFPS = TargetFps, PlayerID = _peerIds[peer.Id] }.Serialize(writer); 
                peer.Send(writer, DeliveryMethod.ReliableOrdered);  
            }

            _checksums.Clear();
            _framePacker = new FramePacker();

            timer.Start();

            Console.WriteLine("Simulation started");

            while (Running)
            {       
                timer.Tick();

                accumulatedTime += timer.DeltaTime;

                while (accumulatedTime >= dt)
                {   
                    writer.Reset();
                    writer.Put((byte)MessageTag.Frame);
                    _framePacker.Pack(writer);     

                    Distribute(writer);   

                    accumulatedTime -= dt;
                }

                Thread.Sleep(1);
            }

            Console.WriteLine("Simulation stopped");    
        }
    }
}
