using Lockstep.Core.Services;
using Simulation.Behaviour.Actor;
using Simulation.Behaviour.Debugging;
using Simulation.Behaviour.Features;
using Simulation.Behaviour.Game;
using Simulation.Behaviour.GameState;

namespace Simulation.Behaviour
{
    public sealed class SimulationSystems : Feature
    {
        public SimulationSystems(Contexts contexts, ServiceContainer services)
        {
            Add(new InitializeEntityCount(contexts));

            Add(new OnNewPredictionCreateBackup(contexts, services));

            Add(new InputFeature(contexts, services));

            Add(new VerifySelectionIdExists(contexts, services));

            Add(new NavigationFeature(contexts, services));

            Add(new GameEventSystems(contexts));

            Add(new CalculateHashCode(contexts, services));

            Add(new RemoveNewFlag(contexts));

            Add(new IncrementTick(contexts));

            Add(new VerifyNoDuplicateBackups(contexts, services));

        }      
    }
}
