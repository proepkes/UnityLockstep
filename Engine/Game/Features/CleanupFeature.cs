using Lockstep.Game.Features.Cleanup;

namespace Lockstep.Game.Features
{
    sealed class CleanupFeature : Feature
    {
        public CleanupFeature(Contexts contexts, ServiceContainer services) : base("Cleanup")
        {
            Add(new RemoveDestroyedEntitiesFromView(contexts, services));
        }
    }
}
