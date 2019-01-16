using System;
using System.Collections.Generic;
using ECS.Data;
using LiteNetLib.Utils;
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

        private readonly NetDataWriter _buffer = new NetDataWriter();
        private readonly List<SerializedInput> _commands = new List<SerializedInput>();
        private readonly Dictionary<uint, byte[]> _frames = new Dictionary<uint, byte[]>();     

        /// <summary>
        /// Packages all recent frames that fit into one MTU and advances to the next frame
        /// </summary>
        /// <param name="writer"></param>
        public void Pack(NetDataWriter writer)
        {
            _buffer.Reset();

            SerializedInput[] serializedInputs;
            lock (_commands)
            {        
                serializedInputs = _commands.ToArray();
                _commands.Clear();

            }         

            var frame = new Frame { SerializedInputs = serializedInputs}; 
            frame.Serialize(_buffer, _frameCounter);

            _frames.Add(_frameCounter, new byte[_buffer.Length]);
            Array.Copy(_buffer.Data, _frames[_frameCounter], _buffer.Length);


            _buffer.Reset();
            //add previous frames for redundancy
            var countFrames = WriteFrames(_buffer, _frameCounter, MAX_BUFFERSIZE - writer.Length - 8); //MTU - existingBytes - countframes(=4) - bytesLength(=4)
                                                                
            writer.Put(countFrames);
            writer.PutBytesWithLength(_buffer.Data, 0, _buffer.Length);

            _frameCounter++;
        }

        public void AddCommand(SerializedInput serializedInput)
        {             
            lock (_commands)
            {
                _commands.Add(serializedInput);
            }
        }

        private uint WriteFrames(NetDataWriter buffer, uint currentFrame, int maxSize)
        {
            uint i = 0;
            while (i <= currentFrame)
            {
                var nextBufferSize = _frames[currentFrame - i].Length + 4;
                if (buffer.Length + nextBufferSize > maxSize)
                {
                    break;
                }

                buffer.PutBytesWithLength(_frames[currentFrame - i]); //Frames put in descending order (newest first)

                i++;
            }

            return i;
        }
    }
}
