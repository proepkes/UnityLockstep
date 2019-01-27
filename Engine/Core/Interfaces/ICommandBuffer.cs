using System;                     
using Lockstep.Core.Data;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        void Lock();
        void Release();

        long NextFrameIndex { get; }          

        void Insert(byte commanderId, long frameNumber, ICommand[] commands);

        ICommand[] GetNext();
    }
}