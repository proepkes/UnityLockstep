using Lockstep.Core.Services;

namespace Lockstep.Game.Services
{
    public interface IViewService : IService
    {                                                        
        void LoadView(GameEntity entity, int configId);

        void DeleteView(uint entityId);
    }
}