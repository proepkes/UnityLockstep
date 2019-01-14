using System;
using System.Collections.Generic;
using LiteNetLib.Utils;
using Lockstep.Framework.Networking.Messages;       

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
        private readonly List<Command> _commands = new List<Command>();
        private readonly Dictionary<uint, byte[]> _frames = new Dictionary<uint, byte[]>();     

        /// <summary>
        /// Packages all recent frames that fit into one MTU and advances to the next frame
        /// </summary>
        /// <param name="writer"></param>
        public void Pack(NetDataWriter writer)
        {
            _buffer.Reset();

            Command[] commands;
            lock (_commands)
            {        
                commands = _commands.ToArray();
                _commands.Clear();

            }         

            var frame = new Frame {FrameNumber = _frameCounter++, Commands = commands}; 
            frame.Serialize(_buffer);

            _frames.Add(frame.FrameNumber, new byte[_buffer.Length]);
            Array.Copy(_buffer.Data, _frames[frame.FrameNumber], _buffer.Length);


            _buffer.Reset();
            //add previous frames for redundancy
            var countFrames = WriteFrames(_buffer, frame.FrameNumber, MAX_BUFFERSIZE - writer.Length - 8); //MTU - existingBytes - countframes(=4) - bytesLength(=4)
                                                                
            writer.Put(countFrames);
            writer.PutBytesWithLength(_buffer.Data, 0, _buffer.Length);                                                     
        }

        public void AddCommand(Command command)
        {             
            lock (_commands)
            {
                _commands.Add(command);
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
