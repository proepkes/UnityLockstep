using System;
using System.Collections.Generic;
using System.IO;
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;       
using Lockstep.Core.Interfaces;
using Lockstep.Network;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;

namespace Lockstep.Client
{
    public class Simulation
    {                                       
        public event Action<long> Ticked;

        public bool Running { get; set; }
                                                                      
        private readonly ISystems _systems;
        private readonly INetwork _network;

        public float _tickDt;
        public float _accumulatedTime;

        public long CurrentTick { get; private set; }

        public ICommandBuffer LocalCommandBuffer => _systems.CommandBuffer;
        public readonly CommandBuffer NetworkCommandBuffer;

        private readonly IDictionary<ushort, Func<ISerializableCommand>> _commandFactories = new Dictionary<ushort, Func<ISerializableCommand>>();   

        public Simulation(ISystems systems, INetwork network)
        {
            _systems = systems;
            _systems.CommandBuffer = new CommandBuffer();

            NetworkCommandBuffer = new CommandBuffer();

            _network = network;
            _network.DataReceived += OnDataReceived;         
        }


        public void RegisterCommand(Func<ISerializableCommand> commandFactory)
        {
            var tag = commandFactory.Invoke().Tag;
            if (_commandFactories.ContainsKey(tag))
            {
                throw new InvalidDataException("The command tag " + tag + " is already registered. Every command tag must be unique.");
            }
            _commandFactories.Add(tag, commandFactory);   
        }

        public void Execute(ISerializableCommand command)
        {         
            //Execute the command locally on the next tick
            _systems.CommandBuffer.Insert(CurrentTick + 1, command);
            
            //Tell the server
            var writer = new Serializer();
            writer.Put((byte)MessageTag.Input);
            writer.Put(CurrentTick + 1);
            writer.Put(command.Tag);
            command.Serialize(writer);

            _network.Send(Compressor.Compress(writer));  
        }

        public void Update(float deltaTime)
        {                             
            if (!Running)                        
            {
                return;
            }        

            _accumulatedTime += deltaTime;
                                                            
            while (_accumulatedTime >= _tickDt)
            {            
                Tick();          

                _accumulatedTime -= _tickDt;
            }                 
        }

        private void StartSimulation(int targetFps)
        {
            _tickDt = 1000f / targetFps;

            _systems.Initialize();

            Running = true;                         
        }         

        private void Tick()
        {                             
            _systems.Tick();
            Ticked?.Invoke(CurrentTick);

            CurrentTick++;
        }

        private void OnDataReceived(byte[] data)
        {
            data = Compressor.Decompress(data);

            var reader = new Deserializer(data);
            var messageTag = (MessageTag)reader.GetByte();
            switch (messageTag)
            {
                case MessageTag.StartSimulation:
                    var init = new Init();
                    init.Deserialize(reader);
                    StartSimulation(init.TargetFPS);
                    break;
                case MessageTag.Input:
                    var frameNumber = reader.GetLong();   
                    var tag = reader.GetUShort();

                    if (_commandFactories.ContainsKey(tag))
                    {
                        var newCommand = _commandFactories[tag].Invoke();
                        newCommand.Deserialize(reader);

                        NetworkCommandBuffer.Insert(frameNumber, newCommand);
                    }  
                                                                                    
                    break;
            }
        }
    }
}