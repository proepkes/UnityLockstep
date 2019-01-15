using System;         
using BEPUutilities;
using ECS.Data;       
using LiteNetLib.Utils;
using Lockstep.Framework;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Services;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Framework.Test
{
    public class SimulationTest
    {                                               
        public SimulationTest(ITestOutputHelper output)
        {                                        
            Console.SetOut(new Converter(output));
        }

        [Fact]
        public void TestCommandService()
        {       
            var commandService = new Mock<ICommandService>();
            var timeService = new Mock<ITimeService>();

            var sim = new Simulation();
            sim.Init(commandService.Object, timeService.Object, 0);
                                 
            for (var i = 0; i < 10; i++)
            {
                var command = new Command();
                sim.AddFrame(new Frame { Commands = new[] { command } });
                sim.Simulate();

                commandService.Verify(service => service.Process(It.IsAny<GameContext>(), command), Times.Exactly(1));
            }
        }

        [Fact]
        public void TestGameEntityHasDestinationAfterNavigationCommand()
        {
            var contexts = Contexts.sharedInstance;     

            var commandService = new DefaultCommandService();
            var timeService = new Mock<ITimeService>();
            var destination = new Vector2(11, 22);

            var sim = new Simulation();
            sim.Init(commandService, timeService.Object, 0);      

            var e = contexts.game.CreateEntity();

            var serializer = new NetDataWriter();
            serializer.Put((byte) CommandTag.Navigate);
            new NavigateCommand { Destination = destination, EntityIds = new []{ e.id.value }}.Serialize(serializer);   

            sim.AddFrame(new Frame { Commands = new Command[0] });
            sim.Simulate();

            e.hasDestination.ShouldBeFalse();

            var command = new Command { Data = serializer.Data };
            sim.AddFrame(new Frame { Commands = new[] { command } });
            sim.Simulate();

            e.hasDestination.ShouldBeTrue();
            e.destination.value.X.ShouldBe(destination.X);
            e.destination.value.Y.ShouldBe(destination.Y);
        }                         
    }
}
