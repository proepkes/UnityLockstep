using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUutilities;

namespace Lockstep.Core.Interfaces
{
    public interface IDebugService : IService
    {
        void Register(uint tick, long hash);
        long GetHash(uint tick);
        bool HasHash(uint tick);

        void Register(uint tick, uint entityId, Vector2 pos);

        bool Validate(uint tick, uint entityId, Vector2 pos);
    }
}
