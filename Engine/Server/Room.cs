using System;
using System.Collections.Generic;
using System.Threading;
using LiteNetLib.Utils;
using Lockstep.Network;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;

namespace Server
{
    public class Room
    {
        private const int TargetFps = 20;

        private byte _nextPlayerId;
        private readonly int _size;

        private InputPacker _inputPacker; 
        private readonly IServer _server;

        public bool Running { get; private set; } 

        /// <summary>
        /// Mapping: clientId -> playerId
        /// </summary>
        private readonly Dictionary<int, byte> _playerIds = new Dictionary<int, byte>();

        /// <summary>
        /// Mapping: Framenumber -> received hashcode
        /// </summary>
        private readonly Dictionary<ulong, long> _hashCodes = new Dictionary<ulong, long>();

        public Room(IServer server, int size)
        {
            _server = server;
            _size = size;
        }    
        public void Open(int port)
        {
            _server.ClientConnected += OnClientConnected;
            _server.ClientDisconnected += OnClientDisconnected;
            _server.DataReceived += OnDataReceived;

            _server.Run(port);

            Console.WriteLine("Server started. Waiting for " + _size + " players...");
        }

        private void OnClientConnected(int clientId)
        {
            _playerIds.Add(clientId, _nextPlayerId++);

            if (_playerIds.Count == _size)
            {
                Console.WriteLine("Room is full, starting new simulation...");
                new Thread(Loop) { IsBackground = true }.Start();
            }
            else
            {
                Console.WriteLine(_playerIds.Count + " / " + _size + " players have connected.");
            }
        }   

        private void OnDataReceived(int clientId, byte[] data)
        {
            var reader = new Deserializer(data);
            var messageTag = (MessageTag)reader.GetByte();
            switch (messageTag)
            {
                case MessageTag.Input:
                    _inputPacker?.AddInput(reader.GetRemainingBytes());
                    break;
                case MessageTag.HashCode:                                 
                    var pkt = new HashCode();
                    pkt.Deserialize(reader);
                    if (!_hashCodes.ContainsKey(pkt.FrameNumber))
                    {
                        _hashCodes[pkt.FrameNumber] = pkt.Value;
                    }
                    else
                    {
                        Console.WriteLine((_hashCodes[pkt.FrameNumber] == pkt.Value ? "HashCode valid" : "Desync") + ": " + pkt.Value);
                    }
                    break;
            }      
        }

        private void OnClientDisconnected(int clientId)
        {
            _playerIds.Remove(clientId);
            if (_playerIds.Count == 0)
            {
                Console.WriteLine("All players left, stopping current simulation...");
                Running = false;
            }
            else
            {
                Console.WriteLine(_playerIds.Count + " players remaining.");
            }
        }              

        private void Loop()
        {    
            var timer = new Timer();
            var dt = 1000.0 / TargetFps;

            var accumulatedTime = 0.0;

            _hashCodes.Clear();
            var serializer = new Serializer();
            _inputPacker = new InputPacker();

            Running = true; 

            StartSimulationOnConnectedPeers(serializer);

            timer.Start();

            Console.WriteLine("Simulation started");
                                               
            while (Running)
            {       
                timer.Tick();

                accumulatedTime += timer.DeltaTime;

                while (accumulatedTime >= dt)
                {       
                    serializer.Reset();

                    _inputPacker.Pack(serializer);           
                    _server.Distribute(serializer.Data, serializer.Length);   

                    accumulatedTime -= dt;
                }

                Thread.Sleep(1);
            }

            Console.WriteLine("Simulation stopped");
        }

        private void StartSimulationOnConnectedPeers(Serializer writer)
        {                        
            //Create a new seed and send it with a start-message to all clients
            //The message also contains the respective player-id and the servers' frame rate 
            var seed = new Random().Next(int.MinValue, int.MaxValue);

            foreach (var player in _playerIds)
            {
                writer.Reset();
                writer.Put((byte)MessageTag.StartSimulation);
                new Init
                {
                    Seed = seed,
                    TargetFPS = TargetFps,
                    PlayerID = player.Value
                }.Serialize(writer);

                _server.Send(player.Key, writer.Data, writer.Length);
            }   
        }
    }
}
