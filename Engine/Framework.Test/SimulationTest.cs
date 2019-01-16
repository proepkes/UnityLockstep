using System;
using System.Collections.Generic;
using BEPUutilities;
using ECS.Data;
using Entitas;
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

            var sim = new Simulation();
            sim.Init(new List<IService>{ commandService.Object }, 0);
                                 
            for (var i = 0; i < 10; i++)
            {
                var command = new Command();
                sim.AddFrame(new Frame { Commands = new[] { command } });
                sim.Simulate();

                commandService.Verify(service => service.Process(It.IsAny<InputContext>(), command), Times.Exactly(1));
            }
        }

        [Fact]
        public void TestInputEntityAfterNavigationCommand()
        { 
            var commandService = new DefaultCommandService();     

            var destination = new Vector2(11, 22);

            var serializer = new NetDataWriter();
            serializer.Put((byte)CommandTag.Navigate);
            new NavigateCommand { Destination = destination, EntityIds = new int[0] }.Serialize(serializer);

            var command = new Command { Data = serializer.Data };

            var sim = new Simulation();
            sim.Init(new List<IService> { commandService }, 0);
            sim.AddFrame(new Frame { Commands = new[] { command } });
            sim.Simulate();

            var inputEntitiesWithInputPosition = Contexts.sharedInstance.input.GetGroup(InputMatcher.InputPosition);
            inputEntitiesWithInputPosition.count.ShouldBe(1);
            inputEntitiesWithInputPosition.GetSingleEntity().inputPosition.value.X.ShouldBe(destination.X);
            inputEntitiesWithInputPosition.GetSingleEntity().inputPosition.value.Y.ShouldBe(destination.Y);
        }                         
    }
}
