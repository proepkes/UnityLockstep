using System;
using System.Collections.Generic;
using System.IO;
using Lockstep.Core.Commands;
using Lockstep.Network;
using Lockstep.Network.Utils;

namespace Lockstep.Game.Network
{
    public sealed class NetworkCommandBuffer : CommandBuffer, IMessageHandler
    {
        /// <summary>
        /// Amount of additional ticks until a command gets executed
        /// </summary>
        public uint LagCompensation { get; set; }

        private readonly INetwork _network;   
        private readonly IDictionary<ushort, Func<ISerializableCommand>> _commandFactories = new Dictionary<ushort, Func<ISerializableCommand>>();

        public NetworkCommandBuffer(INetwork network)
        {                        
            _network = network;     
        }
                  
        public void RegisterCommand(Func<ISerializableCommand> commandFactory)
        {
            var tag = commandFactory.Invoke().Tag;
            if (_commandFactories.ContainsKey(tag))
            {
                throw new InvalidDataException($"The command tag {tag} is already registered. Every command tag must be unique.");
            }
            _commandFactories.Add(tag, commandFactory);
        }         

        public override void Insert(uint frameNumber, byte commanderId, params ICommand[] commands)
        {                                  
            base.Insert(frameNumber + LagCompensation, commanderId, commands);

            //Tell the server
            var writer = new Serializer();
            writer.Put((byte)MessageTag.Input);
            writer.Put(commanderId);
            writer.Put(frameNumber + LagCompensation);
            writer.Put(commands.Length);
            foreach (var command in commands)
            {
                if (command is ISerializableCommand serializableCommand)
                {
                    writer.Put(serializableCommand.Tag);
                    serializableCommand.Serialize(writer);
                }
            }

            _network.Send(Compressor.Compress(writer));
        }

        public void Handle(MessageTag messageTag, byte[] data)
        {
            var reader = new Deserializer(data);
            switch (messageTag)
            {
                case MessageTag.Input:
                    var commanderId = reader.GetByte();
                    var frameNumber = reader.GetUInt();
                    var countCommands = reader.GetInt();
                    var commands = new ICommand[countCommands];
                    for (var i = 0; i < countCommands; i++)
                    {
                        var tag = reader.GetUShort();
                        if (!_commandFactories.ContainsKey(tag))
                        {
                            continue;
                        }

                        var newCommand = _commandFactories[tag].Invoke();
                        newCommand.Deserialize(reader);
                        commands[i] = newCommand;
                    }

                    base.Insert(frameNumber, commanderId, commands);
                    break;
            }
        }
    }
}
