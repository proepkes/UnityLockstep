using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems.Input;

namespace Lockstep.Core.Features
{                                    
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ServiceContainer serviceContainer, IFrameDataSource dataSource)
        {
            //TODO: Add InputValidationSystem by matching input with playerId  
            Add(new ReadInput(contexts, dataSource));
            Add(new EmitInput(contexts)); 

            Add(new OnSpawnInputDoLoadEntityIntoGame(contexts, serviceContainer.Get<IGameService>()));      
        }
    }
}
