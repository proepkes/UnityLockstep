using System.Collections.Generic;
using Lockstep.Game.Interfaces;

namespace Lockstep.Game.Simulation
{
    public interface ICommandQueue
    {
        void Enqueue(uint frameNumber, byte commanderId, params ICommand[] commands);

        Dictionary<uint, Dictionary<byte, List<ICommand>>> Dequeue();
    }
}