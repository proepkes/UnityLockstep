using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Logic.Serialization;
using Lockstep.Core.Logic.Serialization.Utils;
using Lockstep.Network.Messages;
using Lockstep.Network.Server.Interfaces;

namespace Lockstep.Network.Server
{
    public class StartedEventArgs : EventArgs
    {
        public int SimulationSpeed { get; set; }

        public byte[] ActorIds { get; set; }

        public StartedEventArgs(int simulationSpeed, byte[] actorIds)
        {
            SimulationSpeed = simulationSpeed;
            ActorIds = actorIds;
        }
    }

    public class InputReceivedEventArgs : EventArgs
    {
        public byte ActorId { get; set; }
        public uint Tick { get; set; }

        public InputReceivedEventArgs(byte actorId, uint tick)
        {
            ActorId = actorId;
            Tick = tick;
        }
    }


    /// <summary>
    /// Relays input
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Server determines the default speed of the simulation measured in FPS
        /// </summary>
        private const int SimulationSpeed = 20;


        public event EventHandler<StartedEventArgs> Starting;
        public event EventHandler<StartedEventArgs> Started;
        public event EventHandler<InputReceivedEventArgs> InputReceived;

        public bool Running { get; private set; } 

        /// <summary>
        /// Mapping: clientId -> actorId
        /// </summary>
        private readonly Dictionary<int, byte> _actorIds = new Dictionary<int, byte>();

        /// <summary>
        /// Mapping: Framenumber -> received hashcode
        /// </summary>
        private readonly Dictionary<ulong, long> _hashCodes = new Dictionary<ulong, long>();

        private uint inputMessageCounter = 0;
        private byte _nextPlayerId;
        private readonly int _size;

        private readonly IServer _server;

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
            _actorIds.Add(clientId, _nextPlayerId++);

            if (_actorIds.Count == _size)
            {
                Console.WriteLine("Room is full, starting new simulation...");
                StartSimulationOnConnectedPeers();
            }
            else
            {
                Console.WriteLine(_actorIds.Count + " / " + _size + " players have connected.");
            }
        }   

        private void OnDataReceived(int clientId, byte[] data)
        {
            var reader = new Deserializer(Compressor.Decompress(data));
            var messageTag = (MessageTag)reader.GetByte();
            switch (messageTag)
            {
                case MessageTag.Input:
                    ++inputMessageCounter;

                    var clientTick = reader.GetUInt();
                    reader.GetByte(); //Client's lag-compensation
                    var commandsCount = reader.GetInt();
                    if (commandsCount > 0 || inputMessageCounter % 8 == 0)
                    {
                        _server.Distribute(clientId, data);
                    } 

                    InputReceived?.Invoke(this, new InputReceivedEventArgs(_actorIds[clientId], clientTick));
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
            _actorIds.Remove(clientId);
            if (_actorIds.Count == 0)
            {
                Console.WriteLine("All players left, stopping current simulation...");
                Running = false;
            }
            else
            {
                Console.WriteLine(_actorIds.Count + " players remaining.");
            }
        }           

        private void StartSimulationOnConnectedPeers()
        {
            var writer = new Serializer();

            //Create a new seed and send it with a start-message to all clients
            //The message also contains the respective player-id and the initial simulation speed
            var seed = new Random().Next(int.MinValue, int.MaxValue);


            Starting?.Invoke(this, new StartedEventArgs(SimulationSpeed, _actorIds.Values.ToArray()));
            foreach (var player in _actorIds)
            {
                writer.Reset();
                writer.Put((byte)MessageTag.Init);
                new Init
                {
                    Seed = seed,
                    ActorID = player.Value,
                    AllActors = _actorIds.Values.ToArray(),
                    SimulationSpeed = SimulationSpeed
                }.Serialize(writer);

                _server.Send(player.Key, Compressor.Compress(writer));
            }

            Started?.Invoke(this, new StartedEventArgs(SimulationSpeed, _actorIds.Values.ToArray()));
        }
    }
}
