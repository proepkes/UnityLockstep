using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lockstep.Core.Logic;
using Lockstep.Core.Logic.Interfaces;
using Lockstep.Core.Logic.Serialization;
using Lockstep.Core.Logic.Serialization.Utils;
using Lockstep.Game;
using Lockstep.Network.Messages;                     

namespace Lockstep.Network.Client
{
    public class NetworkCommandQueue : CommandQueue
    {
        public event EventHandler<Init> InitReceived;

        public byte LagCompensation { get; set; }

        private readonly INetwork _network;
        private readonly Dictionary<ushort, Type> _commandFactories = new Dictionary<ushort, Type>();

        public NetworkCommandQueue(INetwork network)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(type => type.GetInterfaces().Any(intf => intf.FullName != null && intf.FullName.Equals(typeof(ICommand).FullName))))
                {
                    var tag = ((ICommand)Activator.CreateInstance(type)).Tag;
                    if (_commandFactories.ContainsKey(tag))
                    {
                        throw new InvalidDataException($"The command tag {tag} is already registered. Every command tag must be unique.");
                    }
                    _commandFactories.Add(tag, type);
                }
            }

            _network = network;
            _network.DataReceived += NetworkOnDataReceived;            
        }

        public override void Enqueue(Input input)
        {
            base.Enqueue(input);

            //Tell the server
            var writer = new Serializer();
            writer.Put((byte)MessageTag.Input);
            writer.Put(input.Tick);
            writer.Put(LagCompensation);
            writer.Put(input.Commands.Count());
            writer.Put(input.ActorId);
            foreach (var command in input.Commands)
            {
                writer.Put(command.Tag);
                command.Serialize(writer);
            }

            _network.Send(Compressor.Compress(writer));
        }


        private void NetworkOnDataReceived(byte[] rawData)
        {   
            var data = Compressor.Decompress(rawData);
             
            var reader = new Deserializer(data);
            var messageTag = (MessageTag)reader.GetByte();
            switch (messageTag)
            {
                case MessageTag.Init:
                    var paket = new Init();
                    paket.Deserialize(reader);
                    InitReceived?.Invoke(this, paket);
                    break;
                case MessageTag.Input:
                    var tick = reader.GetUInt() + reader.GetByte(); //Tick + LagCompensation
                    var countCommands = reader.GetInt();
                    var actorId = reader.GetByte();
                    var commands = new ICommand[countCommands];
                    for (var i = 0; i < countCommands; i++)
                    {
                        var tag = reader.GetUShort();
                        if (!_commandFactories.ContainsKey(tag))
                        {
                            continue;
                        }

                        var newCommand = (ICommand)Activator.CreateInstance(_commandFactories[tag]);
                        newCommand.Deserialize(reader);
                        commands[i] = newCommand;
                    }


                    base.Enqueue(new Input(tick, actorId, commands));
                    break;    
            }
        }   
    }
}