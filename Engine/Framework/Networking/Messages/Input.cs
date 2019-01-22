using System;
using System.Collections.Generic;
using System.Text;
using Lockstep.Framework.Networking.Serialization;

namespace Lockstep.Framework.Networking.Messages
{
    public class Input
    {
        public byte[] Data { get; set; }   

        public void Serialize(ISerializer writer)
        {
            writer.PutBytesWithLength(Data);     
        }

        public void Deserialize(IDeserializer reader)
        {
            Data = reader.GetBytesWithLength(); 
        }
    }
}
