using System;                     
using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        uint NextFrameIndex { get; }          

        void Insert(byte commanderId, uint frameNumber, ICommand[] commands);

        ICommand[] GetNext();
    }
}