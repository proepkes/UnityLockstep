using System;
using System.Collections.Generic;
using System.IO;
using LiteNetLib.Utils;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Network;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;

namespace Lockstep.Client
{
    /// <summary>
    /// This simulation listens for received data from the client and reacts accordingly. 'Executed' commands are first sent to the server.
    /// The final execution is done when the corresponding frame-packet arrives - this adds minimum 1 RTT delay to commands.
    /// </summary>
    public class NetworkedSimulation
    {                        
        private readonly IClient _client;
        private readonly IDictionary<ushort, Func<ISerializableCommand>> _commandFactories = new Dictionary<ushort, Func<ISerializableCommand>>();

        public event EventHandler Started;

        public ISystems Systems { get; }   
                                                       

        public NetworkedSimulation(ISystems systems, IClient client)
        {
            Systems = systems;

            _client = client;  
            _client.DataReceived += ClientOnDataReceived;     
        }

        public NetworkedSimulation RegisterCommand(Func<ISerializableCommand> commandFactory)
        {
            var tag = commandFactory.Invoke().Tag;
            if (_commandFactories.ContainsKey(tag))
            {
                throw new InvalidDataException("The command tag " + tag + " is already registered. Every command tag must be unique.");
            }
            _commandFactories.Add(tag, commandFactory);
            return this;
        }      

        public void Execute(ISerializableCommand command)
        {
            var writer = new Serializer();
            writer.Put((byte) MessageTag.Input);
            writer.Put(command.Tag);
            command.Serialize(writer);
            _client.Send(writer.Data, writer.Length);              
        }

        public void Send(MessageTag messageTag, ISerializable serializable)
        {
            var writer = new Serializer();
            writer.Put((byte)messageTag); 
            serializable.Serialize(writer);
            _client.Send(writer.Data, writer.Length);
        }

        private void ClientOnDataReceived(byte[] data)
        {                          
            var reader = new Deserializer(data);
            var messageTag = (MessageTag)reader.GetByte();
            switch (messageTag)
            {
                case MessageTag.StartSimulation:
                    Systems.Initialize();
                    Started?.Invoke(this, EventArgs.Empty);
                    break;
                case MessageTag.Frame:
                    var commandsLength = reader.GetInt();

                    var commands = new ICommand[commandsLength];    
                    for (var i = 0; i < commandsLength; i++)
                    {
                        var newCommand = _commandFactories[reader.GetUShort()].Invoke();
                        newCommand.Deserialize(reader);
                        commands[i] = newCommand;                               
                    }               

                    Systems.DataSource.Insert(new Frame { Commands = commands });

                    //TODO: only for debugging, frames should be buffered & later executed in fixed update, in case frames don't arrive in time this would halt the simulation
                    Systems.Tick();

                    Send(MessageTag.HashCode, new HashCode { FrameNumber = Systems.DataSource.ItemIndex, Value = Systems.HashCode });
                    break;
            }  
        }             
    }
}