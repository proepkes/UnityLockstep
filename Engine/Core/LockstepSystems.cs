using Lockstep.Core.Features;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core
{
    public sealed class LockstepSystems : Entitas.Systems, ISystems
    {                   
        public long HashCode => _contexts.gameState.hashCode.value;

        private IFrameBuffer _frameBuffer;
        private readonly Contexts _contexts;

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

        public void SetFrameBuffer(IFrameBuffer frameBuffer)
        {
            _frameBuffer = frameBuffer;
        }

        public void Tick()
        {
            _contexts.input.SetFrame(_frameBuffer.GetNext());

            Execute();
            Cleanup();
        }
    }
}     