using Lockstep.Game;
using Lockstep.Game.Services;
using Shouldly;
using Xunit.Abstractions;

namespace Test
{
    public static class TestUtil
    {
        public static void TestReplayMatchesHashCode(GameLog gamelog, uint targetTick, long expectedHashCode, ITestOutputHelper output)
        {
            output.WriteLine("========================================");

            var input = gamelog.Log;

            var contexts = new Contexts();
            var commandBuffer = new CommandQueue();
            var world = new Simulation(contexts, commandBuffer, new DefaultViewService()); 

            world.Start(1, 0, new byte[] { 0, 1 });

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
                contexts.snapshot.GetEntityWithTick(contexts.gameState.tick.value)?.hashCode.value.ShouldBe(contexts.gameState.hashCode.value);

                world.Update(1);
            }

            output.WriteLine("Checking hashcode: " + expectedHashCode);
            contexts.gameState.hashCode.value.ShouldBe(expectedHashCode);
        }
    }
}
