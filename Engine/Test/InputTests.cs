using System;                     
using System.Linq;
using System.Threading;
using BEPUutilities;
using Entitas;
using Lockstep.Client;
using Lockstep.Client.Implementations; 
using Lockstep.Core;                  
using Lockstep.Core.Interfaces;    
using Lockstep.Network.Messages;  
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
        public void TestCreateEntityRollbackLocal()
        {                      
            var contexts = new Contexts();   

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };

            sim.Initialize(new Init { TargetFPS = 1, AllActors = new byte[] { 0, 1 }, ActorID = 0 });

            systems.CurrentTick.ShouldBe((uint)0);

            sim.Update(1000); //0          
            systems.CurrentTick.ShouldBe((uint)1);

            sim.Execute(new Spawn()); 

            sim.Update(1000); //1            
            systems.CurrentTick.ShouldBe((uint)2);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            sim.Update(1000); //2             
            systems.CurrentTick.ShouldBe((uint)3);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            commandBuffer.Insert(1, 1, new ICommand[] { });
                                                                 
            sim.Update(1000); //3                
            systems.CurrentTick.ShouldBe((uint)4);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);         

            commandBuffer.Insert(2, 1, new ICommand[] { new MoveAll(contexts.game, 1) }); 

            sim.Update(1000); //4                
            systems.CurrentTick.ShouldBe((uint)5);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 2);

            sim.Execute(new Spawn());         

            sim.Update(1000); //5        
            systems.CurrentTick.ShouldBe((uint)6);

            ExpectEntityCount(contexts, 2);
            ExpectBackupCount(contexts, 2); 

            sim.Execute(new Spawn());     

            sim.Update(1000); //6
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 2);
                                                  
            commandBuffer.Insert(3, 1, new ICommand[] { }); //Revert to 3

            sim.Update(1000);
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 3); 
                                                                                               
            sim.Update(1000);
            commandBuffer.Insert(4, 1, new ICommand[] { });
            sim.Execute(new Spawn());    

            sim.Update(1000);
            ExpectEntityCount(contexts, 4);
            ExpectBackupCount(contexts, 4);

            sim.Update(1000);
            sim.Update(1000);
            commandBuffer.Insert(5, 1, new ICommand[] { new Spawn() });

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

            commandBuffer.Insert(2, 1, new ICommand[] { new MoveAll(contexts.game, 1) });       

            sim.Update(1000); //4                           
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);       
           
            sim.Update(1000); //5                           
            systems.CurrentTick.ShouldBe(frameCounter++);

            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);

            sim.Execute(new MoveEntitesOfSpecificActor(contexts.game, 0, Vector2.Zero));

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
            ExpectBackupCount(contexts, 3);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 1);

            commandBuffer.Insert(4, 1, new ICommand[] { new Spawn() }); //Revert to 4

            sim.Update(1000); //8                          
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 6);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 2);

            commandBuffer.Insert(5, 1, new ICommand[] { new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn() }); //Revert to 5

            sim.Update(1000);                           
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 9);
            ExpectBackupCount(contexts, 15);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            sim.Update(1000);
            systems.CurrentTick.ShouldBe(frameCounter++);
            sim.Update(1000);
            systems.CurrentTick.ShouldBe(frameCounter++);
            sim.Update(1000);
            systems.CurrentTick.ShouldBe(frameCounter++);   

            sim.Execute(new Spawn());
            commandBuffer.Insert(6, 1, new ICommand[] { new MoveEntitesOfSpecificActor(contexts.game, 1, Vector2.Zero)  }); 

            sim.Update(1000);
            ExpectEntityCount(contexts, 10);
            ExpectBackupCount(contexts, 24); 
            GameEntityCountMatchesActorEntityCount(contexts, 0, 2);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            sim.Execute(new Spawn());
            commandBuffer.Insert(11, 1, new ICommand[] { new MoveEntitesOfSpecificActor(contexts.game, 1, new Vector2(3,4)) });
            sim.Update(1000);

            ExpectEntityCount(contexts, 11);
            ExpectBackupCount(contexts, 33); 
            GameEntityCountMatchesActorEntityCount(contexts, 0, 3);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);
        }
        [Fact]
        public void TestGameLogReplay()
        {
            var contexts = new Contexts();

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };

            sim.Initialize(new Init { TargetFPS = 1, AllActors = new byte[] { 0, 1 }, ActorID = 0 });     

            sim.Update(1000); //0                          

            sim.Execute(new Spawn());

            sim.Update(1000); //1     
            sim.Update(1000); //2    
            sim.Update(1000); //3                                                   

            commandBuffer.Insert(2, 1, new ICommand[] { new MoveAll(contexts.game, 1) });

            sim.Update(1000); //4   
            sim.Update(1000); //5                         

            sim.Execute(new MoveEntitesOfSpecificActor(contexts.game, 0, Vector2.Zero));

            sim.Update(1000); //6                                                    

            commandBuffer.Insert(3, 1, new ICommand[] { new Spawn() }); //Revert to 3

            sim.Update(1000); //7                                                     

            commandBuffer.Insert(4, 1, new ICommand[] { new Spawn() }); //Revert to 4

            sim.Update(1000); //8                                                    

            commandBuffer.Insert(5, 1, new ICommand[] { new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn() }); //Revert to 5

            sim.Update(1000);        
            sim.Update(1000);                            
            sim.Update(1000);                              
            sim.Update(1000);

            sim.Execute(new Spawn());
            sim.Execute(new MoveAll(contexts.game, 0));
            commandBuffer.Insert(6, 1, new ICommand[] { new MoveEntitesOfSpecificActor(contexts.game, 1, Vector2.Zero) });

            sim.Update(1000);                                                        

            sim.Execute(new Spawn());
            commandBuffer.Insert(11, 1, new ICommand[] { new MoveEntitesOfSpecificActor(contexts.game, 1, new Vector2(3, 4)) });
            sim.Update(1000);

            _output.WriteLine("========================================");

            var input = systems.GameLog.Log;
            var finalTick = systems.CurrentTick;
            var finalHash = contexts.gameState.hashCode.value;

            commandBuffer.Buffer.ShouldBeEmpty();
            var debug = systems.Services.Get<IDebugService>();

            contexts.Reset();
            systems = new World(contexts, new TestLogger(_output));
            sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };
            sim.Initialize(new Init { TargetFPS = 1, AllActors = new byte[] { 0, 1 }, ActorID = 0 });

            foreach (var (occurTickId, tickCommands) in input)
            {
                foreach (var (tickId, allCommands) in tickCommands)
                {
                    foreach (var (actorId, commands) in allCommands)
                    {          
                        if (actorId == 0)
                        {
                            _output.WriteLine("Local: " + commands.Count + " commands");

                            systems.AddInput(tickId, actorId, commands);
                        }
                        else
                        {
                            commandBuffer.Insert(tickId, actorId, commands.ToArray());
                        }
                    }
                }
            }

            var debug2 = systems.Services.Get<IDebugService>();
            debug.ShouldNotBeSameAs(debug2);


            while (systems.CurrentTick < finalTick)
            {
                sim.Update(1);
                if (debug.HasHash(systems.CurrentTick))
                {
                    debug.GetHash(systems.CurrentTick).ShouldBe(contexts.gameState.hashCode.value);
                }
            }

            contexts.gameState.hashCode.value.ShouldBe(finalHash);
        }
        [Fact]
        //This test requires a navigation-service that just sets the position to destination: entity.ReplacePosition(entity.destination.value);
        public void TestCommandUsesCorrectEntityIds()
        {
            var contexts = new Contexts();

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };

            sim.Initialize(new Init { TargetFPS = 1, AllActors = new byte[] { 0, 1 }, ActorID = 0 });
                                 

            sim.Update(1000);   
            sim.Update(1000);
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Update(1000);
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Update(1000);    

            var selection = new uint[] { 0, 1, 3, 13, 18 };
            var destination = new Vector2(14 , 15);

            commandBuffer.Insert(1, 1, new ICommand[] {
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn() });

            commandBuffer.Insert(8, 1, new ICommand[] {
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),
                new Spawn(),     
                new Spawn() });
            commandBuffer.Insert(9, 1, new ICommand[]
            {
                new MoveSelection(selection, destination)                       
            });

            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Execute(new Spawn());
            sim.Update(1000);

            sim.Execute(new MoveSelection(new uint[]{1,4,2,8,3}, destination));
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            contexts.game.GetEntities(GameMatcher.Id).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeUnique();
            contexts.game.GetEntities(GameMatcher.Id)
                .Where(entity => entity.actorId.value == 1 && selection.Contains(entity.id.value))
                .Select(entity => entity.position.value).ShouldAllBe(vector2 => vector2.X == destination.X && vector2.Y == destination.Y);      

            destination = new Vector2(5, 15);
            commandBuffer.Insert(10, 1, new ICommand[]
            {
                new MoveSelection(selection, destination)
            });

            sim.Update(1000);
            contexts.game.GetEntities(GameMatcher.Id).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeUnique();
            contexts.game.GetEntities(GameMatcher.Id)
                .Where(entity => entity.actorId.value == 1 && selection.Contains(entity.id.value))
                .Select(entity => entity.position.value).ShouldAllBe(vector2 => vector2.X == destination.X && vector2.Y == destination.Y);                              
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

        public class MoveAll : ICommand
        {
            private readonly GameContext _contexts;
            private readonly byte _actorId;
            private uint[] selection;

            public MoveAll(GameContext contexts, byte actorId)
            {
                _contexts = contexts;
                _actorId = actorId;
                selection = _contexts.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == _actorId)
                    .Select(entity => entity.id.value).ToArray();
            }

            public void Execute(InputEntity e)
            {
                e.AddSelection(selection);
                e.AddCoordinate(new Vector2(2, 2)); 
            }
        }


        public class MoveEntitesOfSpecificActor : ICommand
        {                                    
            private readonly GameContext _contexts;
            private readonly byte _actorId;
            private readonly Vector2 _dest;
            private uint[] selection;

            public MoveEntitesOfSpecificActor(GameContext contexts, byte actorId, Vector2 dest)
            {
                _contexts = contexts;
                _actorId = actorId;
                _dest = dest;

                selection = _contexts.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == _actorId).Select(entity => entity.id.value).ToArray();
            }

            public void Execute(InputEntity e)
            {
                e.AddSelection(selection);
                e.AddCoordinate(_dest);
                e.AddTargetActorId(_actorId); 
                
            }  
        }

        public class MoveSelection : ICommand
        {                                            
            private readonly uint[] _selection;
            private readonly Vector2 _destination;

            public MoveSelection(uint[] selection, Vector2 destination)
            {                         
                _selection = selection;
                _destination = destination;
            }

            public void Execute(InputEntity e)
            {                             
                e.AddSelection(_selection);
                e.AddCoordinate(_destination);
            }
        }
    }

}
