using System;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Lockstep.Framework.Networking.Messages
{
    /// <summary>
    /// Class to deserialize a the data from <see cref="FramePacker"/>
    /// </summary>
    public class FramePackage
    {
        public FramePackage(uint maxFrames = 0)
        {
            MaxFrames = maxFrames;
        }

        public uint CountFrames { get; set; }

        public Frame[]  Frames { get; set; }

        /// <summary>
        /// Defines how many frames should be deserialized. 0 means all that were received
        /// </summary>
        public uint MaxFrames { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(NetDataReader reader)
        {
            CountFrames = reader.GetUInt();
            var buffer = reader.GetBytesWithLength();

            var frames = new List<Frame>();          

            var bufferReader = new NetDataReader(buffer);

            CountFrames = MaxFrames == 0 ? CountFrames : Math.Min(CountFrames, MaxFrames);

            for (var i = 0; i < CountFrames; i++)
            {
                var frame = new Frame();
                frame.Deserialize(new NetDataReader(bufferReader.GetBytesWithLength()));
                frames.Add(frame);   
            }

            Frames = frames.ToArray();
        }
    }
}
