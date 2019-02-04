namespace Lockstep.Core.Interfaces
{
    public interface ILogService : IService
    {
        void Warn(object message);
        void Trace(object message);       
    }
}