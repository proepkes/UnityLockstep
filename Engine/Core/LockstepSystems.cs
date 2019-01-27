using Lockstep.Core.Data;
using Lockstep.Core.Features;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core
{
    public sealed class LockstepSystems : Entitas.Systems, ISystems
    {                      
        public Contexts Contexts { get; }                     

        public LockstepSystems(Contexts contexts, params IService[] additionalServices)
        {
            Contexts = contexts;
            Contexts.SubscribeId();

            var serviceContainer = new ServiceContainer();
            foreach (var service in additionalServices)
            {
                serviceContainer.Register(service);
            }

            Add(new InputFeature(Contexts, serviceContainer));

            Add(new NavigationFeature(Contexts, serviceContainer));

            Add(new GameEventSystems(Contexts));

            Add(new HashCodeFeature(Contexts, serviceContainer));
        }         

        public void SetInput(ICommand[] input)
        {
            Contexts.input.SetCommands(input);
        }

        public void Tick()
        {        
            Execute();
            Cleanup();
        }
    }
}     