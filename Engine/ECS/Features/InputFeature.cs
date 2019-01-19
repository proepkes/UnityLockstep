using System;
using System.Collections.Generic;
using System.Linq;      
using ECS.Systems.Input;

namespace ECS.Features
{
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ICollection<IService> externalServices)
        {    
            Add(new EmitInputSystem(contexts, externalServices.FirstOrDefault(service => service is IParseInputService) as IParseInputService));
            Add(new OnInputCreateGameEntity(contexts));
            Add(new OnGameEntityLoadAsset(contexts, externalServices.FirstOrDefault(service => service is IViewService) as IViewService));
            Add(new OnInputSetDestination(contexts, externalServices.FirstOrDefault(service => service is ILogger) as ILogger));
        }
    }
}
