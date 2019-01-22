using ECS.Systems.Input;

namespace ECS.Features
{                                    
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            //TODO: Add InputValidationSystem by matching input with playerId


            Add(new ReadInput(contexts, serviceContainer.Get<IInputService>()));
            Add(new EmitInput(contexts)); 

            Add(new OnSpawnInputDoLoadEntityIntoGame(contexts, serviceContainer.Get<IGameService>()));
            Add(new OnNavigableEntityDoAddAgent(contexts, serviceContainer.Get<INavigationService>()));

            Add(new OnNavigationInputDoSetAgentDestination(contexts, serviceContainer.Get<INavigationService>()));
        }
    }
}
