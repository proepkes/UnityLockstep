using Lockstep.Core.Logic.Systems.Actor;
using Lockstep.Core.Logic.Systems.Debugging;      
using Lockstep.Core.Logic.Systems.GameState;            

namespace Lockstep.Core.Logic.Systems
{
    public sealed class WorldSystems : Feature
    {
        public WorldSystems(Contexts contexts, params Feature[] features)
        {
            Add(new InitializeEntityCount(contexts));

            Add(new OnNewPredictionCreateSnapshot(contexts));

            foreach (var feature in features)
            {
                Add(feature);
            }                                               

            Add(new GameEventSystems(contexts));

            Add(new CalculateHashCode(contexts));   

            //Performance-hit, only use for serious debugging
            //Add(new VerifyNoDuplicateBackups(contexts));              

            Add(new DestroyDestroyedGameSystem(contexts));

            Add(new IncrementTick(contexts));
        }      
    }
}
