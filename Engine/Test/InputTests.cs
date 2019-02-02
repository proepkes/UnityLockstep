using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BEPUutilities;
using Entitas;
using Lockstep.Client;
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;
using Lockstep.Core;
using Lockstep.Core.DefaultServices;
using Lockstep.Core.Interfaces;
using Lockstep.Core.Systems;
using Lockstep.Network.Messages;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Test
{    
    public class InputParseTest
    {
        private readonly ITestOutputHelper _output;

        public InputParseTest(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(new Converter(output));
        }
            
        [Fact]
        public void TestGameEntityHasUniqueId()
        {

            var contexts = new Contexts();  

            const int numEntities = 10;

            for (uint i = 0; i < numEntities; i++)
            {
                contexts.game.CreateEntity();
            }

            contexts.game.count.ShouldBe(numEntities);
            contexts.game.GetEntities().Select(entity => entity.hasId).ShouldAllBe(b => true);
            contexts.game.GetEntities().Select(entity => entity.id.value).ShouldBeUnique();
        }
                                                                                                                                                   
        [Fact]
        public void TestCreateEntityRollbackLocal()
        {                      
            var contexts = new Contexts();   

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };

            sim.Initialize(new Init { TargetFPS = 1});


            systems.CurrentTick.ShouldBe((uint)0);

            sim.Update(1000); //0          
            systems.CurrentTick.ShouldBe((uint)1);
            for (int i = 0; i < 1; i++)
            {
                sim.Execute(new Spawn());
            }

            sim.Update(1000); //1            
            systems.CurrentTick.ShouldBe((uint)2);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);

            sim.Update(1000); //2             
            systems.CurrentTick.ShouldBe((uint)3);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);

            commandBuffer.Insert(1, 1, new ICommand[] { });
                                                                 
            sim.Update(1000); //3                
            systems.CurrentTick.ShouldBe((uint)4);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);   


            commandBuffer.Insert(2, 1, new ICommand[] { new MoveAll(contexts.game), });


            sim.Update(1000); //4                
            systems.CurrentTick.ShouldBe((uint)5);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 2);

            for (int i = 0; i < 1; i++)
            {
                sim.Execute(new Spawn());
            }

            sim.Update(1000); //5        
            systems.CurrentTick.ShouldBe((uint)6);

            ExpectEntityCount(contexts, 2);
            ExpectBackupCount(contexts, 3);


            for (int i = 0; i < 1; i++)
            {
                sim.Execute(new Spawn());
            }

            sim.Update(1000); //6
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 4);
                                                  
            commandBuffer.Insert(3, 1, new ICommand[] { }); //Revert to 3

            sim.Update(1000);
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 4); 
                                                                                               
            sim.Update(1000);
            commandBuffer.Insert(4, 1, new ICommand[] { });
            for (int i = 0; i < 1; i++)
            {
                sim.Execute(new Spawn());
            }
            sim.Update(1000);
            ExpectEntityCount(contexts, 4);
            ExpectBackupCount(contexts, 5);

            sim.Update(1000);
            sim.Update(1000);
            for (int i = 0; i < 1; i++)
            {
                commandBuffer.Insert(5, 1, new ICommand[] { new Spawn() });
            }
            sim.Update(1000);


            ExpectEntityCount(contexts, 5);
        }

        [Fact]
        public void TestCreateEntityRollbackRemote()
        {                 
            var contexts = new Contexts();

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };

            sim.Initialize(new Init { TargetFPS = 1, AllActors = new byte[] { 0, 1 }, ActorID = 0 });

            sim.Update(1000);
            for (int i = 0; i < 10; i++)
            {
                sim.Execute(new Spawn());
            }
            sim.Update(1000); 
            sim.Update(1000);
            sim.Update(1000);      
            sim.Update(1000);   

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(2, 1, new ICommand[] { new Spawn() });
            }              

            sim.Update(1000);                     

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(4, 1, new ICommand[] { new Spawn() });
            }

                                       
            for (int i = 0; i < 100; i++)
            {
                sim.Update(1000);
            }                          

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(5, 1, new ICommand[] { new Spawn() });
            }

            sim.Update(1000);    
            sim.Update(1000);
            sim.Update(1000);


            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(7, 1, new ICommand[] { new Spawn() });
            }
            sim.Update(1000);

            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
                                                                  
            _output.WriteLine("Revert to 3");
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(8, 1, new ICommand[] { new Spawn() });
            }

            sim.Update(1000);                                   
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(9, 1, new ICommand[] { new Spawn() });
            }
            sim.Update(1000);

            ExpectEntityCount(contexts, 70);
        }        

        [Fact]
        public void TestEntityRollbackWithLocalChanges()
        {
            var contexts = new Contexts();

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };

            sim.Initialize(new Init { TargetFPS = 1, AllActors = new byte[] { 0, 1 }, ActorID = 0 });

            uint frameCounter = 0;

            systems.CurrentTick.ShouldBe(frameCounter++);

            sim.Update(1000); //0                           
            systems.CurrentTick.ShouldBe(frameCounter++);

            sim.Execute(new Spawn());

            sim.Update(1000); //1                           
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            sim.Update(1000); //2                             
            systems.CurrentTick.ShouldBe(frameCounter++); 

            sim.Update(1000); //3                            
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 0); 

            commandBuffer.Insert(2, 1, new ICommand[] { new MoveAll(contexts.game), });       

            sim.Update(1000); //4                           
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);       
           
            sim.Update(1000); //5                           
            systems.CurrentTick.ShouldBe(frameCounter++);

            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);

            sim.Execute(new MoveEntitesOfSpecificActor(contexts.game, 0));

            sim.Update(1000); //6                          
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 0);                                     

            commandBuffer.Insert(3, 1, new ICommand[] { new Spawn() }); //Revert to 3

            sim.Update(1000); //7                          
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 2);
            ExpectBackupCount(contexts, 2);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 1);

            commandBuffer.Insert(4, 1, new ICommand[] { new Spawn() }); //Revert to 4

            sim.Update(1000); //8                          
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 4);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 2);

            commandBuffer.Insert(5, 1, new ICommand[] { new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn() }); //Revert to 5

            sim.Update(1000);                           
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 9);
            ExpectBackupCount(contexts, 7);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            sim.Update(1000);
            systems.CurrentTick.ShouldBe(frameCounter++);
            sim.Update(1000);
            systems.CurrentTick.ShouldBe(frameCounter++);
            sim.Update(1000);
            systems.CurrentTick.ShouldBe(frameCounter++);   

            sim.Execute(new Spawn());
            commandBuffer.Insert(6, 1, new ICommand[] { new MoveEntitesOfSpecificActor(contexts.game, 1),  }); 

            sim.Update(1000);
            ExpectEntityCount(contexts, 10);
            ExpectBackupCount(contexts, 16); 
            GameEntityCountMatchesActorEntityCount(contexts, 0, 2);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            sim.Execute(new Spawn());
            commandBuffer.Insert(11, 1, new ICommand[] { new MoveEntitesOfSpecificActor(contexts.game, 1), });
            sim.Update(1000);

            ExpectEntityCount(contexts, 11);
            ExpectBackupCount(contexts, 25); //Last frame 11 shadows + 1 newShadow from player 0 + 8 move-shadows from player 1
            GameEntityCountMatchesActorEntityCount(contexts, 0, 3);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);
        }

        [Fact]
        public void TestThread()
        {
            var contexts = new Contexts();

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };

            sim.Initialize(new Init { TargetFPS = 10, AllActors = new byte[] { 0, 1 }, ActorID = 0 });
            sim.StartAsThread();

            Thread.Sleep(1000);

            systems.CurrentTick.ShouldBeInRange(9u, 11u);
        }

        private void ExpectEntityCount(Contexts contexts, int value)
        {
            contexts.game.GetEntities(GameMatcher.LocalId).Length.ShouldBe(value); 
        }

        private void ExpectBackupCount(Contexts contexts, int value)
        {
            contexts.game.GetEntities(GameMatcher.Backup).Length.ShouldBe(value);
        }

        private void GameEntityCountMatchesActorEntityCount(Contexts contexts, byte actorId, int expectedCount)
        {
            var gameEntityCount = contexts.game.GetEntities(GameMatcher.LocalId).Count(entity => entity.actorId.value == actorId);
                gameEntityCount.ShouldBe(expectedCount);
                gameEntityCount.ShouldBe((int)contexts.actor.GetEntities(ActorMatcher.Id).First(actor => actor.id.value == actorId).entityCount.value);
        }


        public class Spawn : ICommand
        {                             
            public int EntityConfigId;

            public Vector2 Position;

            public void Execute(InputEntity e)
            {                                   
                e.AddCoordinate(Position);
                e.AddEntityConfigId(EntityConfigId);   
            }  
        }

        //Hacky commands, don't do this in production. Commands should only modify the given input-entity
        public class MoveAll : ICommand
        {                               
            private readonly GameContext _contexts;

            public MoveAll(GameContext contexts)
            {
                _contexts = contexts;
            }

            public void Execute(InputEntity e)
            {
                foreach (var gameEntity in _contexts.GetEntities(GameMatcher.LocalId))
                {
                    gameEntity.ReplacePosition(new Vector2(2, 2));
                }
            }     
        }

        public class MoveEntitesOfSpecificActor : ICommand
        {                                    
            private readonly GameContext _contexts;
            private readonly byte _actorId;

            public MoveEntitesOfSpecificActor(GameContext contexts, byte actorId)
            {
                _contexts = contexts;
                _actorId = actorId;
            }

            public void Execute(InputEntity e)
            {
                foreach (var gameEntity in _contexts.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == _actorId))
                {
                    gameEntity.ReplacePosition(new Vector2(2, 2));
                }
            }

        }
    }
}
