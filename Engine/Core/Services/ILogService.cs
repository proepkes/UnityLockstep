using System;

namespace Lockstep.Core.Services
{
    public interface ILogService : IService
    {
        void Warn(Func<string> message);
        void Trace(Func<string> message);       
    }
}