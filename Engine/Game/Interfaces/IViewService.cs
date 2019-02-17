namespace Lockstep.Game.Interfaces
{
    public interface IViewService : IService
    {                                                        
        void Instantiate(GameEntity entity, int configId);

        void Destroy(uint entityId);
    }
}