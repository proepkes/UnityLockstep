using System.Collections.Generic;

namespace Lockstep.Core.Logic.Interfaces
{
    public interface ICommandQueue
    {
        void Enqueue(Input input);

        List<Input> Dequeue();
    }
}