using Lockstep.Core.Services;
using Lockstep.Core.Systems.Input;

namespace Lockstep.Core.Features
{                                    
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            //TODO: Add InputValidationSystem
            
            Add(new ExecuteSpawnInput(contexts, serviceContainer));


            //TODO: Add CleanupInput that removes input of validated frames (no rollback required => can be removed)
        }
    }
}
