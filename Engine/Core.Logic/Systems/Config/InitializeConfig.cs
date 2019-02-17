using Entitas;

namespace Lockstep.Core.Logic.Systems.Actor
{
    public class InitializeConfig : IInitializeSystem
    {
        private readonly ConfigContext _configContext;

        public InitializeConfig(Contexts contexts)
        {
            _configContext = contexts.config;
        }
        public void Initialize()
        {
            _configContext.SetNavigationTimeStep(0.5m);
        }
    }
}
