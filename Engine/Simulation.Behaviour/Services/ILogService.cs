using System;
using Lockstep.Core.Services;

namespace Simulation.Behaviour.Services
{
    public interface ILogService : IService
    {
        void Warn(Func<string> message);
        void Trace(Func<string> message);       
    }
}