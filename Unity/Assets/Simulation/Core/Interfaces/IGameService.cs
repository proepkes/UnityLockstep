namespace Lockstep.Core.Interfaces
{
    public interface IGameService : IService
    {                                                        
        void LoadEntity(GameEntity entity, int configId);

        void UnloadEntity(uint entityId);
    }
}