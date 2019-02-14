using System.Linq;
using Lockstep.Game;
using Lockstep.Game.DefaultServices;
using Shouldly;
using Xunit.Abstractions;

namespace Test
{
    public static class TestUtil
    {
        public static void TestReplayMatchesHashCode(Contexts origin, GameLog gamelog, ITestOutputHelper output)
        {
            output.WriteLine("========================================");

            uint targetTick = origin.gameState.tick.value;
            long expectedHashCode = origin.gameState.hashCode.value;


            var input = gamelog.InputLog;
            var contexts = new Contexts();
            var commandBuffer = new CommandQueue();
            var world = new Simulation(contexts, commandBuffer, new DefaultViewService());

            world.Start(1, gamelog.LocalActorId, gamelog.AllActorIds);

            foreach (var (_, tickCommands) in input)
            {
                foreach (var (tickId, allCommands) in tickCommands)
                {
                    foreach (var (actorId, commands) in allCommands)
                    {
                        commandBuffer.Enqueue(tickId, actorId, commands.ToArray());
                    }
                }
            }

            while (contexts.gameState.tick.value < targetTick)
            {
                var debugEntity = origin.debug.GetEntities().FirstOrDefault(entity => entity.tick.value == contexts.gameState.tick.value);
                if (debugEntity != null)
                {
                    debugEntity.hashCode.value.ShouldBe(contexts.gameState.hashCode.value);
                }

                world.Update(1000);
            }

            output.WriteLine("Checking hashcode: " + expectedHashCode);
            contexts.gameState.hashCode.value.ShouldBe(expectedHashCode);
        }
    }
}
