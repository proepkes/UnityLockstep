using Entitas;
using Lockstep.Core.Logic.Systems.Actor;
using Lockstep.Core.Logic.Systems.Debugging;      
using Lockstep.Core.Logic.Systems.GameState;            

namespace Lockstep.Core.Logic.Systems
{
    public sealed class WorldSystems : Feature
    {
        public WorldSystems(Contexts contexts, ServiceContainer services, params Feature[] features)
        {
            Add(new InitializeEntityCount(contexts));

            Add(new OnNewPredictionCreateBackup(contexts, services));

            foreach (var feature in features)
            {
                Add(feature);
            }                                               

            Add(new GameEventSystems(contexts));

            Add(new CalculateHashCode(contexts, services));     

            Add(new IncrementTick(contexts));

            Add(new VerifyNoDuplicateBackups(contexts, services));              

            Add(new DestroyDestroyedGameSystem(contexts));

        }      
    }
}
