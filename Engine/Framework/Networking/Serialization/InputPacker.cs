using System.Collections.Generic;
using ECS.Data;                     
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

        public void  Pack(ISerializer serializer)
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



    /// <summary>
    /// Class to deserialize a the data from <see cref="InputPacker"/>
    /// </summary>
    public class FramePackage
    {
        public FramePackage(uint maxFrames = 0)
        {
            MaxFrames = maxFrames;
        }

        public uint CountFrames { get; set; }

        public Frame[] Frames { get; set; }

        /// <summary>
        /// Defines how many frames should be deserialized. 0 means all that were received
        /// </summary>
        public uint MaxFrames { get; set; }

        public void Deserialize(IDeserializer reader)
        {          
            //var frame = new Frame();
            //frame.Deserialize(reader); 
            //Frames = new[] {frame};

            //CountFrames = reader.GetUInt();
            //var buffer = reader.GetBytesWithLength();

            //var frames = new List<Frame>();

            //var bufferReader = new NetDataReader(buffer);

            //CountFrames = MaxFrames == 0 ? CountFrames : Math.Min(CountFrames, MaxFrames);

            //for (var i = 0; i < CountFrames; i++)
            //{
            //    var frame = new Frame();
            //    frame.Deserialize(new NetDataReader(bufferReader.GetBytesWithLength()));
            //    frames.Add(frame);
            //}

            //Frames = frames.ToArray();
        }
    }
}
