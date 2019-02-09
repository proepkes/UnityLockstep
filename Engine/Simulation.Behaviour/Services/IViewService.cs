using Lockstep.Core.Services;

namespace Simulation.Behaviour.Services
{
    public interface IViewService : IService
    {                                                        
        void LoadView(GameEntity entity, int configId);

        void DeleteView(uint entityId);
    }
}