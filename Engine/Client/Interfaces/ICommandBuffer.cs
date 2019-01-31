using System.Collections.Generic;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;

namespace Lockstep.Client.Interfaces
{
    public interface ICommandBuffer
    {                                                                         
        uint LastInsertedFrame { get; }           

        void Insert(TickId frame, PlayerId commanderId, ICommand[] commands);

        Dictionary<PlayerId, List<ICommand>> Get(TickId frame);

        //ICommand[] GetMany(uint frame);
    }
}