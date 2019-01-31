using Lockstep.Core.Systems.Input;

namespace Lockstep.Core.Features
{                                    
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            //TODO: Add InputValidationSystem using input's playerIdComponent
            
            Add(new OnSpawnInputCreateEntity(contexts, serviceContainer));

            //Add(new CleanupInput(contexts));
        }

    }
}
