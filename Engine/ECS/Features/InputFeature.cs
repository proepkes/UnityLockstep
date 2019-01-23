using ECS.Systems.Input;

namespace ECS.Features
{                                    
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            //TODO: Add InputValidationSystem by matching input with playerId  
            Add(new ReadInput(contexts, serviceContainer.Get<IDataSource>()));
            Add(new EmitInput(contexts)); 

            Add(new OnSpawnInputDoLoadEntityIntoGame(contexts, serviceContainer.Get<IGameService>()));      
        }
    }
}
