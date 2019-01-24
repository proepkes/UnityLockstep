using System.Collections.Generic;     
using ECS;
using ECS.Data;

namespace Lockstep.Framework
{
    class FrameDataSource : IFrameDataSource
    {                                   
        private readonly Dictionary<uint, Frame> _frames = new Dictionary<uint, Frame>();  

        public uint Count { get; private set; }

        public uint ItemIndex { get; private set; }

        public void Insert(Frame item)
        {
            lock (_frames)
            {
                _frames[Count++] = item;
            }              
        }

        public Frame GetNext()
        {
            Frame nextFrame;
            lock (_frames)
            {
                nextFrame = _frames[ItemIndex++];
            }
            return nextFrame;
        }
    }
}
