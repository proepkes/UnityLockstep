using System;
using System.Collections.Generic;
using System.IO;
using Lockstep.Game.Commands;
using Lockstep.Network;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;

namespace Lockstep.Game.Networking
{
    public sealed class NetworkCommandBuffer : CommandBuffer
    {
        //TODO: refactor: don't do meta stuff through commandbuffer just because it has IClient
        public event Action<Init> InitReceived;   

        private readonly IClient _client;   
        private readonly IDictionary<ushort, Func<ISerializableCommand>> _commandFactories = new Dictionary<ushort, Func<ISerializableCommand>>();

        public NetworkCommandBuffer(IClient client)
        {                        
            _client = client;     
            _client.DataReceived += OnDataReceived;
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

        public override void Insert(uint frameNumber, byte commanderId, ICommand[] commands)
        {                                                     
            //Tell the server
            var writer = new Serializer();
            writer.Put((byte)MessageTag.Input);
            writer.Put(commanderId);
            writer.Put(frameNumber);
            writer.Put(commands.Length);
            foreach (var command in commands)
            {
                if (command is ISerializableCommand serializableCommand)
                {
                    writer.Put(serializableCommand.Tag);
                    serializableCommand.Serialize(writer);
                }
            }

            _client.Send(Compressor.Compress(writer));
        }


        private void OnDataReceived(byte[] data)
        {
            data = Compressor.Decompress(data);

            var reader = new Deserializer(data);
            var messageTag = (MessageTag)reader.GetByte();
            switch (messageTag)
            {                  
                case MessageTag.Init:
                    var init = new Init();
                    init.Deserialize(reader);
                    InitReceived?.Invoke(init);
                    break;
                case MessageTag.Input:
                    var commanderId = reader.GetByte();
                    var frameNumber = reader.GetUInt();
                    var countCommands = reader.GetInt();
                    var commands = new ICommand[countCommands];
                    for (var i = 0; i < countCommands; i++)
                    {
                        var tag = reader.GetUShort();

                        if (_commandFactories.ContainsKey(tag))
                        {
                            var newCommand = _commandFactories[tag].Invoke();
                            newCommand.Deserialize(reader);
                            commands[i] = newCommand;
                        }
                    }

                    base.Insert(frameNumber, commanderId, commands); 
                    break;
            }
        }    
    }
}
