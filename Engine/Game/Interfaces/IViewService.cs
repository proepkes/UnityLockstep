namespace Lockstep.Game.Interfaces
{
    public interface IViewService : IService
    {                                                        
        void LoadView(GameEntity entity, int configId);

        void DeleteView(uint entityId);
    }
}