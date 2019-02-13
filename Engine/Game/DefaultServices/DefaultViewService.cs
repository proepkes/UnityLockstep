using Lockstep.Game.Interfaces;

namespace Lockstep.Game.DefaultServices
{
    public class DefaultViewService : IViewService
    {           
        public void LoadView(GameEntity entity, int configId)
        {
            entity.isNavigable = true;
        }

        public void DeleteView(uint entityId)
        {
            
        }     
    }
}