using System.Collections.Generic;

namespace Lockstep.Core.Commands
{
    public interface ICommandBuffer
    {
        Dictionary<uint, Dictionary<byte, List<ICommand>>> Buffer { get; }
                                             
        void Insert(uint frame, byte commanderId, params ICommand[] commands);
                                                            
        Dictionary<uint, Dictionary<byte, List<ICommand>>> GetChanges();
    }
}