using System;                          
using System.Linq;    
using BEPUutilities;
using ECS.Data;
using ECS.Features;
using LiteNetLib.Utils;
using Lockstep.Framework;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Services;
using Lockstep.Framework.Services.Pathfinding;
using Moq;        
using Shouldly;       
using Xunit;
using Xunit.Abstractions;

namespace Framework.Test
{
    public class PathfindingTest
    {
        private readonly ITestOutputHelper _output;

        public PathfindingTest(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(new Converter(output));
        }  


        [Fact]
        public void TestRvo()
        {
            var serializer = new NetDataWriter();
            var destination = new Vector2(111, 22);   

            var container = new ServiceContainer();              
            container.Register<IParseInputService>(new ParseInputService());
            container.Register<IPathfindingService>(new RVOPathfinderService());
            container.Register<ILogger>(new TestLogger(_output));
            container.Register(new Mock<IViewService>().Object);

            new SpawnCommand
            {
                Movable = true
            }.Serialize(serializer);

            //Initialize a new simulation and add a gameentity by adding a spawncommand to the input
            var sim = new Simulation(container)
                .Init(0)
                .AddFrame(new Frame
                {
                    SerializedInputs = new[]
                    {
                        new SerializedInput { Data = serializer.Data }
                    }
                })
                .Simulate();

            //The SpawnCommand taught the system to create a new gameEntity. Now navigate it
            var e = Contexts.sharedInstance.game.GetEntities().First();

            _output.WriteLine(e.position.value.ToString());

            serializer.Reset();                              
            new NavigateCommand
            {
                Destination = destination,
                EntityIds = new[] { e.id.value }
            }.Serialize(serializer);

            sim.AddFrame(new Frame
                {
                    SerializedInputs = new[]
                    {
                        new SerializedInput { Data = serializer.Data }
                    }
                })
                .Simulate();


            for (int i = 0; i < 500; i++)
            {
                sim.AddFrame(new Frame()).Simulate();
            }    
            _output.WriteLine(e.position.value.ToString());

            e.hasDestination.ShouldBeFalse();
        }


        [Fact]
        public void TestSpawnAndNavigateEntity()
        {                       
            var destination = new Vector2(11, 22);

            var serializer = new NetDataWriter();        
            new SpawnCommand().Serialize(serializer);

            var container = new ServiceContainer();   
            container.Register<IParseInputService>(new ParseInputService());
            container.Register<IPathfindingService>(new SimplePathfinderService());
            container.Register<ILogger>(new TestLogger(_output));
            container.Register(new Mock<IViewService>().Object);   

            //Initialize a new simulation and add a gameentity
            var sim = new Simulation(container)
                .Init(0)
                .AddFrame(new Frame
                {
                    SerializedInputs = new[]
                    {
                        new SerializedInput { Data = serializer.Data }
                    }
                })
                .Simulate();

            //Navigate the new created entity
            var e = Contexts.sharedInstance.game.GetEntities().First();

            serializer.Reset();                               
            new NavigateCommand { Destination = destination, EntityIds = new[] { e.id.value } }.Serialize(serializer);

            sim.AddFrame(new Frame
                {
                    SerializedInputs = new[]
                    {
                        new SerializedInput { Data = serializer.Data }
                    }
                })
                .Simulate();

            Contexts.sharedInstance.input.GetEntities().Length.ShouldBe(0); //There should be no inputs after a simulation was executed

            Contexts.sharedInstance.game.GetGroup(GameMatcher.Destination).GetEntities().Length.ShouldBe(1);

            var gameEntity = Contexts.sharedInstance.game.GetGroup(GameMatcher.Destination).GetSingleEntity();
            gameEntity.destination.value.X.ShouldBe(destination.X);
            gameEntity.destination.value.Y.ShouldBe(destination.Y);
        }
    }
}
