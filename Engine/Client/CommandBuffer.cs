using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;

namespace Lockstep.Client.Implementations
{                
    public class CommandBuffer : ICommandBuffer
    {                                   
        private readonly Dictionary<long, List<ICommand>> _buffer = new Dictionary<long, List<ICommand>>();

        public event Action<byte, long, ICommand[]> Inserted;

        public long Count
        {
            get
            {
                lock (_buffer)
                {
                    return _buffer.LongCount();
                }
            }
        }

        public long ItemIndex { get; private set; }

        public long Remaining => Count - ItemIndex;

        public virtual void Insert(byte commanderId, long frameNumber, ICommand[] commands)
        {
            lock (_buffer)
            {
                if (!_buffer.ContainsKey(frameNumber))
                {
                    _buffer.Add(frameNumber, new List<ICommand>(10));
                }

                _buffer[frameNumber].AddRange(commands);

                Inserted?.Invoke(commanderId, frameNumber, commands);
            }              
        }

        public ICommand[] GetNext()
        {                    
            lock (_buffer)
            {
                //If no commands were inserted then return an empty list
                if (!_buffer.ContainsKey(ItemIndex))
                {
                    _buffer[ItemIndex] = new List<ICommand>();
                }

                return _buffer[ItemIndex++].ToArray();    

            }                 
        }
    }
}
