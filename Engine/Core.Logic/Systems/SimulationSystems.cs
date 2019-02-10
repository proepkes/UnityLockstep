using Lockstep.Core.Logic.Systems.Actor;
using Lockstep.Core.Logic.Systems.Debugging;
using Lockstep.Core.Logic.Systems.Features;
using Lockstep.Core.Logic.Systems.Game;
using Lockstep.Core.Logic.Systems.GameState;

namespace Lockstep.Core.Logic.Systems
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
