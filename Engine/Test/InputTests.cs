using System;                     
using System.Linq;
using System.Threading;
using BEPUutilities;
using Entitas;
using Lockstep.Core.Commands;
using Lockstep.Core.Services;
using Lockstep.Core.World;
using Lockstep.Game;
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

            var commandBuffer = new CommandBuffer();
            var world = new Simulation(contexts, commandBuffer, new TestLogger(_output));

            world.Start(1, 0, new byte[] { 0, 1 });

            world.CurrentTick.ShouldBe((uint)0);

            world.Update(1000); //0          
            world.CurrentTick.ShouldBe((uint)1);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());

            world.Update(1000); //1            
            world.CurrentTick.ShouldBe((uint)2);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            world.Update(1000); //2             
            world.CurrentTick.ShouldBe((uint)3);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            commandBuffer.Insert(1, 1);

            world.Update(1000); //3                
            world.CurrentTick.ShouldBe((uint)4);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);         

            commandBuffer.Insert(2, 1, new MoveAll(contexts.game, 0));

            world.Update(1000); //4                
            world.CurrentTick.ShouldBe((uint)5);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 2);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());

            world.Update(1000); //5        
            world.CurrentTick.ShouldBe((uint)6);

            ExpectEntityCount(contexts, 2);
            ExpectBackupCount(contexts, 2);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());

            world.Update(1000); //6
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 2);
                                                  
            commandBuffer.Insert(4, 1); //Revert to 3

            world.Update(1000);
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 3);

            world.Update(1000);
            commandBuffer.Insert(5, 1);
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());

            world.Update(1000);
            ExpectEntityCount(contexts, 4);
            ExpectBackupCount(contexts, 6);

            world.Update(1000);
            world.Update(1000);
            commandBuffer.Insert(6, 1, new Spawn());

            world.Update(1000);    

            ExpectEntityCount(contexts, 5);

            TestUtil.TestReplayMatchesHashCode(world.GameLog, world.CurrentTick, contexts.gameState.hashCode.value,
                world.Services.Get<IDebugService>(), _output);
        }

        [Fact]
        public void TestCreateEntityRollbackRemote()
        {
            var randomPosition = new Random();

            var contexts = new Contexts();

            var commandBuffer = new CommandBuffer();
            var world = new Simulation(contexts, commandBuffer, new TestLogger(_output));

            world.Start(1, 0, new byte[] { 0, 1 });
            

            world.Update(1000);
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200))});
            }
            world.Update(1000); 
            world.Update(1000);
            world.Update(1000);      
            world.Update(1000);   

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(2, 1, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) });
            }              

            world.Update(1000);                     

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(4, 1, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) });
            }

                                       
            for (int i = 0; i < 100; i++)
            {
                world.Update(1000);
            }                          

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(5, 1, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) });
            }

            world.Update(1000);    
            world.Update(1000);
            world.Update(1000);


            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(7, 1, new ICommand[] { new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) } });
            }
            world.Update(1000);

            world.Update(1000);
            world.Update(1000);
            world.Update(1000);
            world.Update(1000);
            world.Update(1000);
                                                                  
            _output.WriteLine("Revert to 3");
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(8, 1, new ICommand[] { new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) } });
            }

            world.Update(1000);                                   
            world.Update(1000);
            world.Update(1000);
            world.Update(1000);
            world.Update(1000);
            for (int i = 0; i < 5; i++)
            {
                commandBuffer.Insert(9, 1, new ICommand[] { new Spawn { Position = new Vector2(randomPosition.Next(0,100), randomPosition.Next(100,200))} });
            }
            world.Update(1000);

            ExpectEntityCount(contexts, 65);

            TestUtil.TestReplayMatchesHashCode(world.GameLog, world.CurrentTick, contexts.gameState.hashCode.value,
                world.Services.Get<IDebugService>(), _output);
        }        

        [Fact]
        public void TestEntityRollbackWithLocalChanges()
        {
            var contexts = new Contexts();

            var commandBuffer = new CommandBuffer();
            var world = new Simulation(contexts, commandBuffer, new TestLogger(_output));

            world.Start(1, 0, new byte[] { 0, 1 });

            world.Update(1000); //0    

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());

            world.Update(1000); //1    
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            world.Update(1000); //2    

            world.Update(1000); //3       
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 0); 

            commandBuffer.Insert(2, 1, new MoveAll(contexts.game, 1));       

            world.Update(1000); //4     
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);       
           
            world.Update(1000); //5    

            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new MoveEntitesOfSpecificActor(contexts.game, 0, Vector2.Zero));

            world.Update(1000); //6    
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 0);                                     

            commandBuffer.Insert(3, 1, new ICommand[] { new Spawn() }); //Revert to 3

            world.Update(1000); //7    
            ExpectEntityCount(contexts, 2);
            ExpectBackupCount(contexts, 3);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 1);

            commandBuffer.Insert(4, 1, new Spawn()); //Revert to 4

            world.Update(1000); //8      
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 6);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 2);

            commandBuffer.Insert(5, 1, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn()); //Revert to 5

            world.Update(1000);        
            ExpectEntityCount(contexts, 9);
            ExpectBackupCount(contexts, 15);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            world.Update(1000);
            world.Update(1000);
            world.Update(1000);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(6, 1, new MoveSelection(new uint[]{ 0, 3, 5 }, new Vector2(8,3))); 

            world.Update(1000);
            ExpectEntityCount(contexts, 10);
            ExpectBackupCount(contexts, 24); 
            GameEntityCountMatchesActorEntityCount(contexts, 0, 2);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(11, 1, new MoveEntitesOfSpecificActor(contexts.game, 1, new Vector2(3,4)));
            world.Update(1000);

            ExpectEntityCount(contexts, 11);
            ExpectBackupCount(contexts, 25); 
            GameEntityCountMatchesActorEntityCount(contexts, 0, 3);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            TestUtil.TestReplayMatchesHashCode(world.GameLog, world.CurrentTick, contexts.gameState.hashCode.value,
                world.Services.Get<IDebugService>(), _output);
        }

        [Fact]
        public void TestGameLogReplay()
        {
            var contexts = new Contexts();

            var commandBuffer = new CommandBuffer();
            var world = new Simulation(contexts, commandBuffer, new TestLogger(_output));

            world.Start(1, 0, new byte[] { 0, 1 });

            world.Update(1000); //0                          

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());

            world.Update(1000); //1     
            world.Update(1000); //2    
            world.Update(1000); //3                                                   

            commandBuffer.Insert(2, 1, new MoveAll(contexts.game, 1));

            world.Update(1000); //4   
            world.Update(1000); //5                         

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new MoveEntitesOfSpecificActor(contexts.game, 0, Vector2.Zero));

            world.Update(1000); //6                                                    

            commandBuffer.Insert(3, 1, new Spawn()); //Revert to 3

            world.Update(1000); //7                                                     

            commandBuffer.Insert(4, 1, new Spawn()); //Revert to 4

            world.Update(1000); //8                                                    

            commandBuffer.Insert(5, 1, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn()); //Revert to 5

            world.Update(1000);        
            world.Update(1000);                            
            world.Update(1000);                              
            world.Update(1000);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new MoveAll(contexts.game, 0));
            commandBuffer.Insert(6, 1, new MoveEntitesOfSpecificActor(contexts.game, 1, Vector2.Zero));

            world.Update(1000);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(11, 1, new MoveEntitesOfSpecificActor(contexts.game, 1, new Vector2(3, 4)));
            world.Update(1000);

            TestUtil.TestReplayMatchesHashCode(world.GameLog, world.CurrentTick, contexts.gameState.hashCode.value,
                world.Services.Get<IDebugService>(), _output);
        }
        [Fact]
        //This test requires a navigation-service that just sets the position to destination: entity.ReplacePosition(entity.destination.value);
        public void TestCommandUsesCorrectEntityIds()
        {
            var contexts = new Contexts();

            var commandBuffer = new CommandBuffer();
            var world = new Simulation(contexts, commandBuffer, new TestLogger(_output));

            world.Start(1, 0, new byte[] { 0, 1 });

            world.Update(1000);   
            world.Update(1000);
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            world.Update(1000);
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            world.Update(1000);    

            var selection = new uint[] { 0, 1, 3, 13 };
            var destination = new Vector2(14 , 15);

            commandBuffer.Insert(1, 1, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn());
            commandBuffer.Insert(8, 1, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn());

            commandBuffer.Insert(9, 1, new MoveSelection(selection, destination));

            world.Update(1000);
            world.Update(1000);
            world.Update(1000);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new Spawn());
            world.Update(1000);

            commandBuffer.Insert(world.CurrentTick, world.LocalActorId, new MoveSelection(new uint[]{1,4,2,8,3}, destination));
            world.Update(1000);
            world.Update(1000);
            world.Update(1000);
            world.Update(1000);


            contexts.game.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeSubsetOf(Enumerable.Range(0, 18).Select(i => (uint)i));
            contexts.game.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeUnique();

            contexts.game.GetEntities(GameMatcher.LocalId)
                .Where(entity => entity.actorId.value == 1 && selection.Contains(entity.id.value))
                .Select(entity => entity.destination.value).ShouldAllBe(vector2 => vector2.X == destination.X && vector2.Y == destination.Y);

            destination = new Vector2(5, 15);
            commandBuffer.Insert(10, 1, new MoveSelection(selection, destination));

            world.Update(1000);
            contexts.game.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeSubsetOf(Enumerable.Range(0, 18).Select(i => (uint)i));
            contexts.game.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeUnique();
            contexts.game.GetEntities(GameMatcher.LocalId)
                .Where(entity => entity.actorId.value == 1 && selection.Contains(entity.id.value))
                .Select(entity => entity.destination.value).ShouldAllBe(vector2 => vector2.X == destination.X && vector2.Y == destination.Y);

            TestUtil.TestReplayMatchesHashCode(world.GameLog, world.CurrentTick, contexts.gameState.hashCode.value,
                world.Services.Get<IDebugService>(), _output);
        }

        //[Fact]
        //public void TestThread()
        //{
        //    var contexts = new Contexts();

        //    var commandBuffer = new CommandBuffer();
        //    var world = new World(contexts, commandBuffer, new TestLogger(_output)) { LagCompensation = 0, SendCommandsToBuffer = false };
        //    world.Initialize(new Init { TargetFPS = 10, AllActors = new byte[] { 0, 1 }, ActorID = 0 });
        //    world.StartAsThread();

        //    Thread.Sleep(1000);

        //    world.CurrentTick.ShouldBeInRange(9u, 11u);
        //}

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

        [Serializable]
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
                e.AddCoordinate(new Vector2(20, 24)); 
                e.AddTargetActorId(_actorId);
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
