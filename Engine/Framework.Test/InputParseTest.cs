using System;                      
using ECS;
using ECS.Data;                   
using Lockstep.Framework;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Networking.Serialization;
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

        [Fact]
        public void TestSpawnInputCreatesEntity()
        {                     
            var contexts = new Contexts();          

            new Simulation(contexts, new ServiceContainer())
                .Init(0)
                .AddFrame(new Frame { Commands = new ICommand[] { new SpawnCommand()  } })
                .Simulate();

            contexts.game.count.ShouldBe(1);  
        }           


        [Fact]
        public void TestInputGetsCalled()
        {
            var command = new Mock<ICommand>();     

            var sim = new Simulation(new Contexts(), new ServiceContainer());
            sim.Init(0);

            for (var i = 0; i < 10; i++)
            {                                       
                sim.AddFrame(new Frame { Commands = new[] { command.Object } });
                sim.Simulate();

                command.Verify(service => service.Execute(It.IsAny<InputContext>()), Times.Exactly(1));
            }
        }
    }
}
