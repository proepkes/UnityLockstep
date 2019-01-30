using Lockstep.Core.Features;   

namespace Lockstep.Core
{
    public sealed class CoreSystems : Entitas.Systems
    {                     
        public CoreSystems(Contexts contexts, ServiceContainer services)
        {                
            Add(new InputFeature(contexts, services));

            //Add(new NavigationFeature(Contexts, serviceContainer));

            Add(new GameEventSystems(contexts));

            Add(new HashCodeFeature(contexts, services));   
        }      
    }
}     