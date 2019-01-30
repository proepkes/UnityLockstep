using System.Collections.Generic;
using Lockstep.Core.Interfaces;

namespace Lockstep.Client.Interfaces
{
    public interface ICommandBuffer
    {                                                                         
        uint LastInsertedFrame { get; }           

        void Insert(uint frameNumber, byte commanderId, ICommand[] commands);

        Dictionary<byte, List<ICommand>> Get(uint frame);

        //ICommand[] GetMany(uint frame);
    }
}