using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECS.Systems;
using ECS.Systems.Input;

namespace ECS.Features
{                                    
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            //TODO: Add InputValidationSystem by matching input with playerId
            Add(new EmitInput(contexts, serviceContainer.Get<IParseInputService>())); 
            Add(new LoadNewEntitiesIntoGame(contexts, serviceContainer.Get<IGameService>()));
        }
    }
}
