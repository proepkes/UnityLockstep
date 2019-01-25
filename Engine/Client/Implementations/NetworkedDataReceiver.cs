using System;
using System.Collections.Generic;
using System.IO;              
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;
using Lockstep.Network;
using Lockstep.Network.Utils;

namespace Lockstep.Client.Implementations
{                       
    public class NetworkedDataReceiver : IDataReceiver
    {
        private readonly INetwork _network;
        private readonly IDictionary<ushort, Func<ISerializableCommand>> _commandFactories = new Dictionary<ushort, Func<ISerializableCommand>>();

        public event EventHandler InitReceived;
        public event EventHandler<Frame> FrameReceived;

        public NetworkedDataReceiver(INetwork network)
        {
            _network = network;
            _network.DataReceived += OnDataReceived;
        }

        /// <summary>
        /// Receives a command by sending it to the server. The server should respond with a frame-package which contains the command. 
        /// </summary>
        /// <param name="command"></param>
        public void Receive(ICommand command)
        {
            if (command is ISerializableCommand serializableCommand)
            {
                var writer = new Serializer();

                writer.Put((byte)MessageTag.Input);
                writer.Put(serializableCommand.Tag);
                serializableCommand.Serialize(writer);

                _network.Send(Compressor.Compress(writer)); 
            }
        }

        public void Receive(MessageTag tag, ISerializable serializable)
        {        
            var writer = new Serializer();

            writer.Put((byte)tag); 
            serializable.Serialize(writer);     

            _network.Send(Compressor.Compress(writer));
        }

        public NetworkedDataReceiver RegisterCommand(Func<ISerializableCommand> commandFactory)
        {
            var tag = commandFactory.Invoke().Tag;
            if (_commandFactories.ContainsKey(tag))
            {
                throw new InvalidDataException("The command tag " + tag + " is already registered. Every command tag must be unique.");
            }
            _commandFactories.Add(tag, commandFactory);
            return this;
        }

        private void OnDataReceived(byte[] data)
        {
            data = Compressor.Decompress(data);

            var reader = new Deserializer(data);
            var messageTag = (MessageTag)reader.GetByte();
            switch (messageTag)
            {
                case MessageTag.StartSimulation:
                    InitReceived?.Invoke(this, EventArgs.Empty);
                    break;
                case MessageTag.Frame:
                    var commandsLength = reader.GetInt();

                    var commands = new ICommand[commandsLength];
                    for (var i = 0; i < commandsLength; i++)
                    {
                        var tag = reader.GetUShort();
                        if (_commandFactories.ContainsKey(tag))
                        {     
                            var newCommand = _commandFactories[tag].Invoke();
                            newCommand.Deserialize(reader);
                            commands[i] = newCommand;
                        }                
                    }

                    FrameReceived?.Invoke(this, new Frame { Commands = commands });
                    break;
            } 
        }
    }
}
