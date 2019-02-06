using Lockstep.Core;
using Lockstep.Core.Services;
using Lockstep.Core.World;
using Lockstep.Game;
using Shouldly;
using Xunit.Abstractions;

namespace Test
{
    public static class TestUtil
    {
        public static void TestReplayMatchesHashCode(GameLog gamelog, uint targetTick, long expectedHashCode, IDebugService debugInfo, ITestOutputHelper output)
        {
            output.WriteLine("========================================");

            var input = gamelog.Log;

            var contexts = new Contexts();
            var commandBuffer = new CommandBuffer();
            var world = new Simulation(contexts, commandBuffer, new TestLogger(output));

            world.Start(1, 0, new byte[] { 0, 1 });

            foreach (var (_, tickCommands) in input)
            {
                foreach (var (tickId, allCommands) in tickCommands)
                {
                    foreach (var (actorId, commands) in allCommands)
                    {
                        commandBuffer.Insert(tickId, actorId, commands.ToArray());
                    }
                }
            }

            while (world.CurrentTick < targetTick)
            {
                world.Update(1);
                if (debugInfo.HasHash(world.CurrentTick))
                {
                    debugInfo.GetHash(world.CurrentTick).ShouldBe(contexts.gameState.hashCode.value);
                }
            }

            output.WriteLine("Checking hashcode: " + expectedHashCode);
            contexts.gameState.hashCode.value.ShouldBe(expectedHashCode);
        }
    }
}
