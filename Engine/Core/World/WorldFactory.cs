using Lockstep.Core.Services;

namespace Lockstep.Core.World
{
    public class WorldFactory
    {
        public static IWorld CreateWorld(Contexts contexts, params IService[] additionalServices)
        {
            var container = new ServiceContainer();
            foreach (var service in additionalServices)
            {
                container.Register(service);
            }

            return new World(contexts, container);
        }
    }
}
