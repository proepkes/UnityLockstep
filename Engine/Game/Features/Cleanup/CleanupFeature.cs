using System;
using System.Collections.Generic;
using System.Text;

namespace Lockstep.Game.Features.Cleanup
{
    sealed class CleanupFeature : Feature
    {
        public CleanupFeature(Contexts contexts, ServiceContainer services) : base("Cleanup")
        {
            Add(new RemoveDestroyedEntitiesFromView(contexts, services));
        }
    }
}
