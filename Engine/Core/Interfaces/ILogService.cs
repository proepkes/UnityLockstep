namespace Lockstep.Core.Interfaces
{
    public interface ILogService : IService
    {
        void Warn(string message);
    }
}