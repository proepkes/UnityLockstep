using System;
using System.Collections.Generic;
using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        Dictionary<uint, Dictionary<byte, List<ICommand>>> Buffer { get; }

        uint LastInsertedFrame { get; }           

        void Insert(uint frameNumber, byte commanderId, ICommand[] commands);

        Dictionary<byte, List<ICommand>> Get(uint frame);

        ICommand[] GetMany(uint frame);
    }
}