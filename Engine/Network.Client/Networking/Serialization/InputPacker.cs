using System.Collections.Generic;        
using Lockstep.Framework.Networking.Messages;

namespace Lockstep.Framework.Networking.Serialization
{                    
    public class InputPacker
    {                                                   
        private readonly List<byte[]> _inputs = new List<byte[]>(); 

        public void AddInput(byte[] serializedInput)
        {
            lock (_inputs)
            {
                _inputs.Add(serializedInput);
            }
        }

        public void Pack(ISerializer serializer)
        {                    
            serializer.Put((byte)MessageTag.Frame);

            byte[][] serializedInputs;
            lock (_inputs)
            {        
                serializedInputs = _inputs.ToArray();
                _inputs.Clear();
            }

            serializer.Put(serializedInputs.Length);
            foreach (var input in serializedInputs)
            {                              
                serializer.Put(input);
            }      
        }       
    }    
}
