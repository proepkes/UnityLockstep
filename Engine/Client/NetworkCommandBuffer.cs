using System;
using System.Collections.Generic;
using System.IO;       
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;         
using Lockstep.Network;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;

namespace Lockstep.Client
{
    public class NetworkCommandBuffer : CommandBuffer
    {
        public event Action<Init> InitReceived;

        private readonly INetwork _network;
        private readonly IDictionary<ushort, Func<ISerializableCommand>> _commandFactories = new Dictionary<ushort, Func<ISerializableCommand>>();

        public NetworkCommandBuffer(INetwork network)
        {                        
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

        public override void Insert(long frameNumber, ICommand command)
        {
            if (command is ISerializableCommand serializable)
            {
                //Tell the server
                var writer = new Serializer();
                writer.Put((byte)MessageTag.Input);
                writer.Put(frameNumber);
                writer.Put(serializable.Tag);
                serializable.Serialize(writer);

                _network.Send(Compressor.Compress(writer));
            }
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
                    InitReceived?.Invoke(init);
                    break;
                case MessageTag.Input:
                    var frameNumber = reader.GetLong();
                    var tag = reader.GetUShort();

                    if (_commandFactories.ContainsKey(tag))
                    {
                        var newCommand = _commandFactories[tag].Invoke();
                        newCommand.Deserialize(reader);


                        base.Insert(frameNumber, newCommand);                               
                    }   

                    break;
            }
        }    
    }
}
