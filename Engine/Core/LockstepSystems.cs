using Lockstep.Core.Data;
using Lockstep.Core.Features;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core
{
    public sealed class LockstepSystems : Entitas.Systems, ISystems
    {
        private readonly Contexts _contexts;      

        public long HashCode => _contexts.gameState.hashCode.value;
        public void SetFrame(Frame frame)
        {
            _contexts.input.ReplaceFrame(frame);
        }

        public LockstepSystems(Contexts contexts, params IService[] additionalServices)
        {                         
            _contexts = contexts;
            _contexts.SubscribeId();

            var serviceContainer = new ServiceContainer();
            foreach (var service in additionalServices)
            {
                serviceContainer.Register(service);
            }

            Add(new InputFeature(contexts, serviceContainer));

            Add(new NavigationFeature(contexts, serviceContainer));

            Add(new GameEventSystems(contexts));

            Add(new HashCodeFeature(contexts, serviceContainer));
        }


        public void Tick()
        {
            Execute();
            Cleanup();
        }
    }
}     