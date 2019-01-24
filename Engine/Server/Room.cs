using System;
using System.Collections.Generic;
using System.Threading;   
using LiteNetLib;                       
using Lockstep.Framework.Networking.Messages;
using Lockstep.Framework.Networking.Serialization;
using Server.LiteNetLib;
using HashCode = Lockstep.Framework.Networking.Messages.HashCode;

namespace Server
{
    public class Room
    {
        public bool Running { get; private set; }    

        private const int TargetFps = 20;
        private const int ServerPort = 9050;       
        private const string ClientKey = "SomeConnectionKey";


        private InputPacker _inputPacker;

        private readonly NetManager _server;
        private readonly EventBasedNetListener _listener;


        private readonly ISerializer _serializer = new LiteNetLibSerializer();
        private readonly IDeserializer _deserializer = new LiteNetLibDeserializer();

        /// <summary>
        /// Mapping: LiteNetLib peerId -> playerId
        /// </summary>
        private readonly Dictionary<int, byte> _playerIds = new Dictionary<int, byte>();

        /// <summary>
        /// Mapping: Framenumber -> received hashcode
        /// </summary>
        private readonly Dictionary<ulong, long> _hashCodes = new Dictionary<ulong, long>();

        public Room()
        {
            _listener = new EventBasedNetListener();
            _server = new NetManager(_listener);

        }            

        public void Distribute(byte[] data)
        {                                        
            foreach (var peer in _server.ConnectedPeerList)
            {                
                peer.Send(data, 0, data.Length, DeliveryMethod.ReliableOrdered);
            }
        }

        public void Open(int roomSize)
        {                 
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
                else
                {
                    Console.WriteLine(_server.PeersCount + " / " + roomSize +" players have connected.");
                }
            };

            _listener.NetworkReceiveEvent += (peer, reader, method) =>
            {
                var messageTag = (MessageTag)reader.GetByte();
                switch (messageTag)
                {
                    case MessageTag.Input:
                        _inputPacker?.AddInput(reader.GetRemainingBytes());
                        break;
                    case MessageTag.HashCode:
                        _deserializer.SetSource(reader.GetRemainingBytes());

                        var pkt = new HashCode();
                        pkt.Deserialize(_deserializer);
                        if (!_hashCodes.ContainsKey(pkt.FrameNumber))
                        {
                            _hashCodes[pkt.FrameNumber] = pkt.Value;
                        }
                        else
                        {
                            Console.WriteLine((_hashCodes[pkt.FrameNumber] == pkt.Value ? "HashCode valid" : "Desync") +": " + pkt.Value); 
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
                else
                { 
                    Console.WriteLine(_server.PeersCount + " players remaining.");
                }
            };

            _server.Start(ServerPort);

            Console.WriteLine("Server started. Waiting for " + roomSize + " players...");
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

            _hashCodes.Clear();
            _serializer.Reset();
            _inputPacker = new InputPacker();

            Running = true; 

            StartSimulationOnConnectedPeers(_serializer);

            timer.Start();

            Console.WriteLine("Simulation started");
                                               
            while (Running)
            {       
                timer.Tick();

                accumulatedTime += timer.DeltaTime;

                while (accumulatedTime >= dt)
                {       
                    _serializer.Reset();

                    _inputPacker.Pack(_serializer);

                    Distribute(_serializer.Data);   

                    accumulatedTime -= dt;
                }

                Thread.Sleep(1);
            }

            Console.WriteLine("Simulation stopped");
        }

        private void StartSimulationOnConnectedPeers(ISerializer writer)
        {                                     
            byte playerId = 0;

            //Create a new seed and send it with a start-message to all clients
            //The message also contains the respective player-id and the servers' frame rate 
            var seed = new Random().Next(int.MinValue, int.MaxValue);
            foreach (var peer in _server.ConnectedPeerList)
            {
                _playerIds[peer.Id] = playerId++;
                                    
                writer.Put((byte) MessageTag.StartSimulation);
                new Init
                {
                    Seed = seed,
                    TargetFPS = TargetFps,
                    PlayerID = _playerIds[peer.Id]
                }.Serialize(writer);

                peer.Send(writer.Data, 0, writer.Length, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
