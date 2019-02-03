using System.Collections.Generic;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        Dictionary<uint, Dictionary<byte, List<ICommand>>> Buffer { get; }

        uint LastInsertedFrame { get; }           

        void Insert(uint frame, byte commanderId, ICommand[] commands);

        Dictionary<byte, List<ICommand>> Get(uint frame); 
    }
}