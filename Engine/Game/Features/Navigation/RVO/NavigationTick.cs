using System.Linq;
using Entitas;
using Lockstep.Common;
using Lockstep.Game.Features.Navigation.RVO.Algorithm;
using Lockstep.Game.Interfaces;

namespace Lockstep.Game.Features.Navigation.RVO
{
    public class NavigationTick : IInitializeSystem, IExecuteSystem
    {
        private readonly Contexts _contexts;
        private readonly INavigationService _navigationService;

        public NavigationTick(Contexts contexts, ServiceContainer services)
        {
            _contexts = contexts;
            _navigationService = services.Get<INavigationService>();
        }
        
        public void Initialize()
        {
            Simulator.Instance.setTimeStep(0.5m);
            Simulator.Instance.setAgentDefaults(15, 10, 5, 5, 1, 1);
        }

        public void Execute()
        {
            foreach (var (_, agent) in Simulator.Instance.agents_)
            {
                agent.CalculatePrefVelocity();
            }
            Simulator.Instance.doStep();


            foreach (var (agentId, agent) in Simulator.Instance.agents_)
            {

                var entity = _contexts.game.GetEntityWithLocalId(agentId);
                entity.ReplacePosition(agent.Position);
            }
        }
    }
}
