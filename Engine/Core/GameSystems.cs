using System;
using Lockstep.Core.Data;
using Lockstep.Core.Features;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems.GameState;
using Lockstep.Core.Systems.Input;

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

        public void RevertToTick(uint tick)
        {
            foreach (var system in _executeSystems)
            {
                if (system is IStateSystem stateSystem)
                {
                    stateSystem.RevertToTick(tick);
                }
            }

            Contexts.gameState.ReplaceTick(tick);   
        }
    }
}     