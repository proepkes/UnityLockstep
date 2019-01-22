using System;                          
using System.Linq;    
using BEPUutilities;
using ECS;
using ECS.Data;
using Framework.Test.LiteNetLib;
using Lockstep.Framework;
using Lockstep.Framework.Commands;   
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
            var destination = new Vector2(111, 22);   

            var container = new ServiceContainer()          
                .Register<ILogService>(new TestLogger(_output));
                                                       

            //Initialize a new simulation and add a gameentity by adding a spawncommand to the input
            var sim = new Simulation(contexts, container)
                .Init(0)
                .AddFrame(new Frame
                {
                    Commands = new ICommand[]
                    {
                        new SpawnCommand()
                    }
                })
                .Simulate();
                                                                                              
            var e = contexts.game.GetEntities().First();
            e.isNavigable = true;

            var before = e.position.value;  

            sim.AddFrame(new Frame
                {
                    Commands = new ICommand[]
                    {
                        new NavigateCommand
                        {
                            Destination = destination,
                            EntityIds = new[] { e.id.value }
                        }
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

            var container = new ServiceContainer()      
                .Register<ILogService>(new TestLogger(_output));   

            //Initialize a new simulation and add a gameentity
            var sim = new Simulation(contexts, container)
                .Init(0)
                .AddFrame(new Frame
                {
                    Commands = new[]
                    {
                        new SpawnCommand()
                    }
                })
                .Simulate();

            //Navigate the new created entity
            var e = contexts.game.GetEntities().First();   

            sim.AddFrame(new Frame
                {
                    Commands = new[]
                    {
                        new NavigateCommand { Destination = destination, EntityIds = new[] { e.id.value } }
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
