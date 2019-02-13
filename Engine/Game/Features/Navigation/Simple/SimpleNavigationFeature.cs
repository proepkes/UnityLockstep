using System;
using System.Collections.Generic;
using System.Text;

namespace Lockstep.Game.Features.Navigation.Simple
{
    sealed class SimpleNavigationFeature : Feature
    {
        public SimpleNavigationFeature(Contexts contexts, ServiceContainer services) : base("SimpleNavigation")
        {
            Add(new NavigationTick(contexts, services));

        }
    }
}
