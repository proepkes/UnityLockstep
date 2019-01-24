using ECS;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;

namespace Lockstep.Core.Features
{
    public sealed class HashCodeFeature : Feature
    {
        public HashCodeFeature(Contexts contexts, ServiceContainer serviceContainer)
        {
            Add(new CalculateHashCode(contexts, serviceContainer.Get<IHashService>()));
        }
    }
}
