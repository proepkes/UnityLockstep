using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECS.Systems;

namespace ECS.Features
{
    public sealed class HashCodeFeature : Feature
    {
        public HashCodeFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            Add(new CalculateHashCode(contexts, serviceContainer.Get<IHashService>()));
        }
    }
}
