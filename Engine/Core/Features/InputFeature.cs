using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems.Input;

namespace Lockstep.Core.Features
{                                    
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            //TODO: Add InputValidationSystem by matching input with playerId  
            Add(new EmitInput(contexts)); 

            Add(new OnSpawnInputDoLoadEntityIntoGame(contexts, serviceContainer.Get<IGameService>()));      
        }
    }
}
