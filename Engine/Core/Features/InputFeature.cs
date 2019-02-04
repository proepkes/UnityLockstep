using Lockstep.Core.Systems.Input;

namespace Lockstep.Core.Features
{                                    
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, Services services)
        {
            //TODO: Add InputValidationSystem
            
            Add(new ExecuteSpawnInput(contexts, services));


            //TODO: Add CleanupInput that removes input of validated frames (no rollback required => can be removed)
            //Add(new CleanupInput(contexts));
        }
    }
}
