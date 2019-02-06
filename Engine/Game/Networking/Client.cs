using System;
using System.Collections.Generic;
using Lockstep.Network;
using Lockstep.Network.Utils;

namespace Lockstep.Game.Networking
{
    public class Client
    {
        private Dictionary<MessageTag, List<IMessageHandler>> messageHandlers = new Dictionary<MessageTag, List<IMessageHandler>>();

        public Client(INetwork network)
        {
            network.DataReceived += NetworkOnDataReceived;            
        }

        private void NetworkOnDataReceived(byte[] rawData)
        {   
            var data = Compressor.Decompress(rawData);
             
            var dataReader = new Deserializer(data);
            var tag = (MessageTag)dataReader.GetByte();
            foreach (var handler in messageHandlers[tag])
            {
                handler.Handle(tag, dataReader.GetRemainingBytes());
            }
        }

        public void AddHandler(IMessageHandler handler, params MessageTag[] tags)
        {
            foreach (var messageTag in tags)
            {
                if (!messageHandlers.ContainsKey(messageTag))
                {
                    messageHandlers.Add(messageTag, new List<IMessageHandler>());
                }
                messageHandlers[messageTag].Add(handler);
            }
        }
    }
}