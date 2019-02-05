using Lockstep.Core.Services;

namespace Lockstep.Game.Services
{
    public interface ILogService : IService
    {
        void Warn(object message);
        void Trace(object message);       
    }
}