using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep.Network;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;

namespace Server
{
    /// <summary>
    /// Relays input
    /// </summary>
    public class Room
    {
        private const int TargetFps = 30;

        private byte _nextPlayerId;
        private readonly int _size;
                                            
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
                StartSimulationOnConnectedPeers();
            }
            else
            {
                Console.WriteLine(_playerIds.Count + " / " + _size + " players have connected.");
            }
        }   

        private void OnDataReceived(int clientId, byte[] data)
        {
            var reader = new Deserializer(Compressor.Decompress(data));
            var messageTag = (MessageTag)reader.PeekByte();
            switch (messageTag)
            {
                case MessageTag.Input:
                    _server.Distribute(clientId, data);    
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

                default:
                    _server.Distribute(data);
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

        private void StartSimulationOnConnectedPeers()
        {
            var writer = new Serializer();

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
                    ActorID = player.Value,
                    AllActors = _playerIds.Values.ToArray(),
                    TargetFPS = TargetFps
                }.Serialize(writer);

                _server.Send(player.Key, Compressor.Compress(writer));
            }   
        }
    }
}
