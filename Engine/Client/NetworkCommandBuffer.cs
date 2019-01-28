using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Network;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;

namespace Lockstep.Client
{
    public class NetworkCommandBuffer : CommandBuffer
    {
        //TODO: refactor: don't receive meta information through commandbuffer
        public event Action<Init> InitReceived;

        private readonly INetwork _network;
        private readonly ILogService _log;
        private readonly IDictionary<ushort, Func<ISerializableCommand>> _commandFactories = new Dictionary<ushort, Func<ISerializableCommand>>();

        public NetworkCommandBuffer(INetwork network, ILogService log)
        {                        
            _network = network;
            _log = log;
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

        public override void Insert(byte commanderId, uint frameNumber, ICommand[] commands)
        {   
            base.Insert(commanderId, frameNumber, commands);

            //Tell the server
            var writer = new Serializer();
            writer.Put((byte)MessageTag.Input);
            writer.Put(commanderId);
            writer.Put(frameNumber);
            writer.Put(commands.Length);
            foreach (var command in commands)
            {
                if (command is ISerializableCommand serializable)
                {    
                    writer.Put(serializable.Tag);
                    serializable.Serialize(writer);
                }
            }

            _network.Send(Compressor.Compress(writer));
        }

        public void Log(uint frameNumber)
        {                     
            var writer = new Serializer();
            writer.Put((byte)MessageTag.Log);    
            writer.Put(frameNumber);        

            _network.Send(Compressor.Compress(writer));
        }

        private void OnDataReceived(byte[] data)
        {
            data = Compressor.Decompress(data);

            var reader = new Deserializer(data);
            var messageTag = (MessageTag)reader.GetByte();
            switch (messageTag)
            {
                case MessageTag.Log:   
                    var n = reader.GetUInt();
                    var count = Buffer.SelectMany(pair => pair.Value).SelectMany(pair => pair.Value).Count();
                    if (count > 0)
                    {
                        _log.Warn(count.ToString());
                    }     
                    break;
                case MessageTag.StartSimulation:
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

                    base.Insert(commanderId, frameNumber, commands); 
                    break;
            }
        }    
    }
}
