using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;

namespace Lockstep.Core.Features
{
    public sealed class HashCodeFeature : Feature
    {
        public HashCodeFeature(Contexts contexts, Services services)
        {
            Add(new CalculateHashCode(contexts, services.Get<IHashService>()));
        }
    }
}
