using System.Collections.Generic;
using ECS.Data;
using Lockstep.Framework.Networking.Serialization;

namespace Lockstep.Framework.Networking
{
    /// <summary>
    /// This class stores all frames and packs as many frames into a package as possible until MTU is reached
    /// </summary>
    public class FramePacker
    {
        private const int MAX_BUFFERSIZE = 1429; // MTU - overhead

        private uint _frameCounter;

        private readonly INetworkWriter _buffer;
        private readonly List<SerializedInput> _inputs = new List<SerializedInput>();
        private readonly Dictionary<uint, byte[]> _frames = new Dictionary<uint, byte[]>();

        public FramePacker(INetworkWriter buffer)
        {
            _buffer = buffer;
        }

        /// <summary>
        /// Packages all recent frames that fit into one MTU and advances to the next frame
        /// </summary>
        /// <param name="writer"></param>
        public byte[] Pack()
        {
            _buffer.Reset();

            _buffer.Put((byte)MessageTag.Frame);

            SerializedInput[] serializedInputs;
            lock (_inputs)
            {        
                serializedInputs = _inputs.ToArray();
                _inputs.Clear();

            }

            var frame = new Frame { SerializedInputs = serializedInputs };
            frame.Serialize(_buffer, _frameCounter++);

            return _buffer.Data;

            //var frame = new Frame { SerializedInputs = serializedInputs}; 
            //frame.Serialize(_buffer, _frameCounter);

            //_frames.Add(_frameCounter, new byte[_buffer.Length]);
            //Array.Copy(_buffer.Data, _frames[_frameCounter], _buffer.Length);


            //_buffer.Reset();
            ////add previous frames for redundancy
            //var countFrames = WriteFrames(_buffer, _frameCounter, MAX_BUFFERSIZE - writer.Length - 8); //MTU - existingBytes - countframes(=4) - bytesLength(=4)

            //writer.Put(countFrames);
            //writer.PutBytesWithLength(_buffer.Data, 0, _buffer.Length);

            //_frameCounter++;
        }

        public void AddInput(SerializedInput serializedInput)
        {
            lock (_inputs)
            {
                _inputs.Add(serializedInput);
            }
        }

        //private uint WriteFrames(NetDataWriter buffer, uint currentFrame, int maxSize)
        //{
        //    uint i = 0;
        //    while (i <= currentFrame)
        //    {
        //        var nextBufferSize = _frames[currentFrame - i].Length + 4;
        //        if (buffer.Length + nextBufferSize > maxSize)
        //        {
        //            break;
        //        }

        //        buffer.PutBytesWithLength(_frames[currentFrame - i]); //Frames put in descending order (newest first)

        //        i++;
        //    }

        //    return i;
        //}
    }

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

        public Frame[] Frames { get; set; }

        /// <summary>
        /// Defines how many frames should be deserialized. 0 means all that were received
        /// </summary>
        public uint MaxFrames { get; set; }

        public void Deserialize(INetworkReader reader)
        {          
            var frame = new Frame();
            frame.Deserialize(reader); 
            Frames = new[] {frame};

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
