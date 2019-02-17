using Lockstep.Game.Interfaces;

namespace Lockstep.Game.DefaultServices
{
    public class DefaultViewService : IViewService
    {           
        public void Instantiate(GameEntity entity, int configId)
        {
            entity.isNavigable = true;
        }

        public void Destroy(uint entityId)
        {
            
        }     
    }
}