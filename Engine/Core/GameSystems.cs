using Lockstep.Core.Features;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems.GameState;    

namespace Lockstep.Core
{
    public sealed class GameSystems : Entitas.Systems, ISystems
    {
        private Contexts Contexts { get; }  

        public ServiceContainer Services { get; }

        public uint CurrentTick => Contexts.gameState.tick.value;

        public GameSystems(Contexts contexts, params IService[] additionalServices)
        {
            Contexts = contexts;      

            Services = new ServiceContainer();
            foreach (var service in additionalServices)
            {
                Services.Register(service);
            }
                                 
            Add(new IncrementTick(Contexts));

            Add(new InputFeature(Contexts, Services));

            //Add(new NavigationFeature(Contexts, serviceContainer));

            Add(new GameEventSystems(Contexts));

            Add(new HashCodeFeature(Contexts, Services));

        }

        public void Tick(ICommand[] input)
        {
            Contexts.input.SetCommands(input);
                        
            Execute();
            Cleanup();
        }

        public void RevertFromTick(uint tick)
        {
            foreach (var system in _executeSystems)
            {
                if (system is IStateSystem stateSystem)
                {
                    stateSystem.RevertFromTick(tick);
                }
            }
            
            //Example: tick = 50, currentTick = 60
            //All ticks from 50 to 60 are reverted
            //Commands for tick 50 are stored one frame behind, in tick 49
            //=> Set new tick to 49 (= tick - 1)
            Contexts.gameState.ReplaceTick(tick - 1);   
        }
    }
}     