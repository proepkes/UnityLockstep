using System;
using BEPUutilities;
using ECS;
using Lockstep.Core.Data;
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
            //var contexts = new Contexts();

            //new Simulation(contexts, new ServiceContainer())
            //    .Init()
            //    .AddFrame(new Frame { Commands = new ICommand[] { new SpawnCommand() } })
            //    .Simulate();

            //contexts.game.count.ShouldBe(1);
        }  

        [Fact]
        public void TestInputGetsCalled()
        {
            //var command = new Mock<ICommand>();     

            //var sim = new Simulation(new Contexts(), new ServiceContainer());
            //sim.Init();
                                  
            //sim.AddFrame(new Frame { Commands = new[] { command.Object } });
            //sim.Simulate(); 

            //command.Verify(c => c.Execute(It.IsAny<InputContext>()), Times.Exactly(1));
        }
    }
}
