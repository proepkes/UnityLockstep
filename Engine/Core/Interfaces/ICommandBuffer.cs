using System;
using System.Collections.Generic;
using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        Dictionary<uint, Dictionary<byte, List<ICommand>>> Buffer { get; }

        uint NextFrameIndex { get; set; }

        void Insert(byte commanderId, uint frameNumber, ICommand[] commands);

        ICommand[] GetNext();
    }
}