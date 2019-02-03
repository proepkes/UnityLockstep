using System.Collections.Generic;

namespace Lockstep.Core.Interfaces
{
    public interface ICommandBuffer
    {
        Dictionary<uint, Dictionary<byte, List<ICommand>>> Buffer { get; }
                                             
        void Insert(uint frame, byte commanderId, ICommand[] commands);
                                                            
        Dictionary<uint, Dictionary<byte, List<ICommand>>> GetChanges();
    }
}