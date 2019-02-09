using System;
using Lockstep.Core.Services;
using Simulation.Behaviour.Services;

namespace Lockstep.Game.Services
{
    public class DefaultLogService : ILogService
    {
        public void Warn(Func<string> message)
        {
            
        }

        public void Trace(Func<string> message)
        {
        }
    }
}
