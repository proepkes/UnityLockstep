using System;
using System.Linq;
using BEPUutilities;
using ECS;
using Lockstep.Client;
using Lockstep.Core;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Moq;
using Shouldly;        
using Xunit;
using Xunit.Abstractions;

namespace Framework.Test
{    
    public class InputParseTest
    {               
        public InputParseTest(ITestOutputHelper output)
        {
            Console.SetOut(new Converter(output));
        }

        class SpawnCommand : ICommand
        {        
            public void Execute(InputContext context)
            {
                context.CreateEntity().AddSpawnInputData(0, 0, Vector2.Zero);
            }
        }

        [Fact]
        public void TestSpawnInputCreatesEntity()
        {
            var contexts = new Contexts();

            var sim = new LocalSimulation(new LockstepSystems(contexts, new ServiceContainer(), new FrameDataSource()));

            var numEntities = 10;

            for (uint i = 0; i < numEntities; i++)
            {
                sim.Execute(new SpawnCommand());    
            }

            contexts.game.count.ShouldBe(numEntities);
            contexts.game.GetEntities().Select(entity => entity.hasId).ShouldAllBe(b => true);
            contexts.game.GetEntities().Select(entity => entity.id.value).ShouldBeUnique();
        }  

        [Fact]
        public void TestCommandIsExecuted()
        {
            var command = new Mock<ICommand>();

            new LocalSimulation(new LockstepSystems(new Contexts(), new ServiceContainer(), new FrameDataSource())).Execute(command.Object);           

            command.Verify(c => c.Execute(It.IsAny<InputContext>()), Times.Exactly(1));
        }

    }
}
