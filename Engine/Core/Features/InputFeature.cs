using System;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems.Input;

namespace Lockstep.Core.Features
{                                    
    public sealed class InputFeature : StatefulFeature
    {
        public InputFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            //TODO: Add InputValidationSystem by matching input with playerId  
            Add(new EmitInput(contexts)); 

            Add(new OnSpawnInputCreateEntity(contexts, serviceContainer));

            Add(new CleanupInput(contexts));
        }

    }
}
