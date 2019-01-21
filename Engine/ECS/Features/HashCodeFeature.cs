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
