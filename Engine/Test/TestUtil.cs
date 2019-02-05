using Lockstep.Core.Services;
using Lockstep.Game;
using Lockstep.Game.Services;
using Lockstep.Network.Messages;
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
            var world = new World(contexts, commandBuffer, new TestLogger(output)) { LagCompensation = 0, SendCommandsToBuffer = false };
            world.Initialize(new Init { TargetFPS = 1, AllActors = new byte[] { 0, 1 }, ActorID = 0 });

            foreach (var (_, tickCommands) in input)
            {
                foreach (var (tickId, allCommands) in tickCommands)
                {
                    foreach (var (actorId, commands) in allCommands)
                    {
                        if (actorId == 0)
                        {
                            output.WriteLine("Local: " + commands.Count + " commands");

                            world.AddInput(tickId, actorId, commands);
                        }
                        else
                        {
                            commandBuffer.Insert(tickId, actorId, commands.ToArray());
                        }
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
