using System;                          
using System.Linq;    
using BEPUutilities;
using ECS;
using ECS.Data;           
using Lockstep.Framework;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Networking.LiteNetLib;
using Lockstep.Framework.Services;      
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
        public void TestSimpleNavigationService()
        {             
            var contexts = new Contexts();
            var serializer = new LiteNetLibNetworkWriter();
            var destination = new Vector2(111, 22);   

            var container = new ServiceContainer()
                .Register<IParseInputService>(new ParseInputService(new LiteNetLibNetworkReader()))          
                .Register<ILogService>(new TestLogger(_output));

            new SpawnCommand().Serialize(serializer);

            //Initialize a new simulation and add a gameentity by adding a spawncommand to the input
            var sim = new Simulation(contexts, container)
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
            var e = contexts.game.GetEntities().First();
            e.isNavigable = true;

            var before = e.position.value;

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

            e.position.value.X.ShouldNotBe(before.X);
            e.position.value.Y.ShouldNotBe(before.Y);
        }


        [Fact]
        public void TestSpawnAndNavigateEntity()
        {
            var contexts = new Contexts();

            var destination = new Vector2(11, 22);

            var serializer = new LiteNetLibNetworkWriter();        
            new SpawnCommand().Serialize(serializer);

            var container = new ServiceContainer()
                .Register<IParseInputService>(new ParseInputService(new LiteNetLibNetworkReader()))       
                .Register<ILogService>(new TestLogger(_output));   

            //Initialize a new simulation and add a gameentity
            var sim = new Simulation(contexts, container)
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
            var e = contexts.game.GetEntities().First();

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

            contexts.input.GetEntities().Length.ShouldBe(0); //There should be no inputs after a simulation was executed

            contexts.game.GetGroup(GameMatcher.Destination).GetEntities().Length.ShouldBe(1);

            var gameEntity = contexts.game.GetGroup(GameMatcher.Destination).GetSingleEntity();
            gameEntity.destination.value.X.ShouldBe(destination.X);
            gameEntity.destination.value.Y.ShouldBe(destination.Y);
        }
    }
}
