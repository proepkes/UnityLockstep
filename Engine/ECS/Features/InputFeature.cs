using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECS.Systems;

namespace ECS.Features
{
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ICollection<IService> externalServices)
        {      

            Add(new EmitInputSystem(contexts, externalServices.FirstOrDefault(service => service is IParseInputService) as IParseInputService));
            Add(new InputToCreateGameEntitySystem(contexts));
            Add(new LoadAssetSystem(contexts, externalServices.FirstOrDefault(service => service is IViewService) as IViewService));
            Add(new InputToGameEntityDestinationSystem(contexts, externalServices.FirstOrDefault(service => service is ILogger) as ILogger));
        }
    }
}
