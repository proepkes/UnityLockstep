using System;
using System.Collections.Generic;
using Client;
using LiteNetLib.Utils;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Network;
using Lockstep.Network.Utils;

namespace Lockstep.Client
{
    public class NetworkedSimulation
    {                                                        
        private readonly IClient _client;    
        private readonly IDictionary<ushort, Func<ISerializableCommand>> _commandFactories = new Dictionary<ushort, Func<ISerializableCommand>>();

        public ISystems Systems { get; }   
                                                       

        public NetworkedSimulation(IClient client, ISystems systems)
        {
            _client = client;         
            Systems = systems;                         
        }

        public NetworkedSimulation RegisterCommand(Func<ISerializableCommand> commandFactory)
        {                           
            _commandFactories.Add(commandFactory.Invoke().Tag, commandFactory);
            return this;
        }


        public void Start(string serverIp, int port)
        {
            _client.DataReceived += ClientOnDataReceived;
            _client.Connect(serverIp, port);  
        }

        public void Execute(ISerializableCommand command)
        {
            var writer = new Serializer();
            writer.Put((byte) MessageTag.Input);
            writer.Put(command.Tag);
            command.Serialize(writer);
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
                    break;
                case MessageTag.Frame:
                                                                
                    var commandsLength = reader.GetInt();

                    var commands = new ICommand[commandsLength];    
                    for (var i = 0; i < commandsLength; i++)
                    {                                             
                        commands[i] =  _commandFactories[reader.GetUShort()].Invoke(); 
                        ((ISerializableCommand)commands[i]).Deserialize(reader);
                    }               

                    Systems.DataSource.Insert(new Frame { Commands = commands });

                    //TODO: only for debugging, frames should be buffered & later executed in fixed update
                    Systems.Tick();
                    break;
            }  
        }             
    }
}