using System;
using System.Linq;
using BEPUutilities;
using ECS.Data;
using Entitas;
using FixMath.NET;
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
        private readonly ITestOutputHelper _output;

        public SimulationTest(ITestOutputHelper output)
        {
            _output = output;

            Console.SetOut(new Converter(output));
        }

        [Fact]
        public void TestCommandService()
        {
            var contexts = Contexts.sharedInstance;

            var commandService = new Mock<ICommandService>();
            var timeService = new Mock<ITimeService>();

            var sim = new Simulation();
            sim.Init(contexts, commandService.Object, timeService.Object, 0);


            uint ticks = 10;

            for (uint i = 0; i < ticks; i++)
            {
                var command = new Command();
                sim.AddFrame(new Frame { Commands = new[] { command } });
                sim.Simulate();

                commandService.Verify(service => service.Process(contexts.game, command), Times.Exactly(1));
            }
        }


        [Fact]
        public void TestGameEntityIsMoving()
        {
            var contexts = Contexts.sharedInstance;

            var commandService = new DefaultCommandService();
            var timeService = new Mock<ITimeService>(); 

            var sim = new Simulation();
            sim.Init(contexts, commandService, timeService.Object, 0);      

            var e = contexts.game.CreateEntity();

            var serializer = new NetDataWriter();
            serializer.Put((byte) CommandTag.Navigate);
            new NavigateCommand { destination = new Vector2(10, 10), entityIds = new []{e.id.value}}.Serialize(serializer); 

            var command = new Command{Data = serializer.Data};

            sim.AddFrame(new Frame { Commands = new[] { command } }); 
            sim.Simulate();
            
            e.isMoving.ShouldBeTrue();
        }                         
    }
}
