using System;                     
using System.Linq;        
using BEPUutilities;
using Entitas;                    
using Lockstep.Core.Logic.Interfaces;
using Lockstep.Core.Logic.Serialization.Utils;          
using Lockstep.Game;
using Lockstep.Game.DefaultServices;
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

            var commandBuffer = new CommandQueue();
            var simulation = new Simulation(contexts, commandBuffer, new DefaultViewService());

            simulation.Start(1, 0, new byte[] { 0, 1 });

            contexts.gameState.tick.value.ShouldBe((uint)0);

            simulation.Update(1000); //0          
            contexts.gameState.tick.value.ShouldBe((uint)1);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());

            simulation.Update(1000); //1            
            contexts.gameState.tick.value.ShouldBe((uint)2);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            simulation.Update(1000); //2             
            contexts.gameState.tick.value.ShouldBe((uint)3);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            commandBuffer.Enqueue(1, 1);

            simulation.Update(1000); //3                
            contexts.gameState.tick.value.ShouldBe((uint)4);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);         

            commandBuffer.Enqueue(2, 1, new MoveAll(contexts.game, 0));

            simulation.Update(1000); //4                
            contexts.gameState.tick.value.ShouldBe((uint)5);
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 2);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());

            simulation.Update(1000); //5        
            contexts.gameState.tick.value.ShouldBe((uint)6);

            ExpectEntityCount(contexts, 2);
            ExpectBackupCount(contexts, 2);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());

            simulation.Update(1000); //6
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 2);
                                                  
            commandBuffer.Enqueue(4, 1); //Revert to 3

            simulation.Update(1000);
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 3);

            simulation.Update(1000);
            commandBuffer.Enqueue(5, 1);
            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());

            simulation.Update(1000);
            ExpectEntityCount(contexts, 4);
            ExpectBackupCount(contexts, 5);

            simulation.Update(1000);
            simulation.Update(1000);
            commandBuffer.Enqueue(6, 1, new Spawn());

            simulation.Update(1000);    

            ExpectEntityCount(contexts, 5);

            TestUtil.TestReplayMatchesHashCode(contexts, simulation.GameLog, _output);
        }

        [Fact]
        public void TestCreateEntityRollbackRemote()
        {
            var randomPosition = new Random();

            var contexts = new Contexts();

            var commandBuffer = new CommandQueue();
            var simulation = new Simulation(contexts, commandBuffer, new DefaultViewService());

            simulation.Start(1, 0, new byte[] { 0, 1 });
            

            simulation.Update(1000);
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200))});
            }
            simulation.Update(1000); 
            simulation.Update(1000);
            simulation.Update(1000);      
            simulation.Update(1000);   

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Enqueue(2, 1, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) });
            }              

            simulation.Update(1000);                     

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Enqueue(4, 1, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) });
            }

                                       
            for (int i = 0; i < 100; i++)
            {
                simulation.Update(1000);
            }                          

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Enqueue(5, 1, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) });
            }

            simulation.Update(1000);    
            simulation.Update(1000);
            simulation.Update(1000);


            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Enqueue(7, 1, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) });
            }
            simulation.Update(1000);

            simulation.Update(1000);
            simulation.Update(1000);
            simulation.Update(1000);
            simulation.Update(1000);
            simulation.Update(1000);
                                                                  
            _output.WriteLine("Revert to 3");
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Enqueue(8, 1, new Spawn { Position = new Vector2(randomPosition.Next(0, 100), randomPosition.Next(100, 200)) });
            }

            simulation.Update(1000);                                   
            simulation.Update(1000);
            simulation.Update(1000);
            simulation.Update(1000);
            simulation.Update(1000);
            for (int i = 0; i < 5; i++)
            {
                commandBuffer.Enqueue(9, 1, new Spawn { Position = new Vector2(randomPosition.Next(0,100), randomPosition.Next(100,200))});
            }
            simulation.Update(1000);

            ExpectEntityCount(contexts, 65);

            TestUtil.TestReplayMatchesHashCode(contexts, simulation.GameLog, _output);
        }        

        [Fact]
        public void TestEntityRollbackWithLocalChanges()
        {
            var contexts = new Contexts();

            var commandBuffer = new CommandQueue();
            var simulation = new Simulation(contexts, commandBuffer, new DefaultViewService());

            simulation.Start(1, 0, new byte[] { 0, 1 });

            simulation.Update(1000); //0    

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());

            simulation.Update(1000); //1    
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            simulation.Update(1000); //2    

            simulation.Update(1000); //3       
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 0);

            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 0); 

            commandBuffer.Enqueue(2, 1, new MoveAll(contexts.game, 1));       

            simulation.Update(1000); //4     
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);       
           
            simulation.Update(1000); //5    

            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new MoveEntitesOfSpecificActor(contexts.game, 0, Vector2.Zero));

            simulation.Update(1000); //6    
            ExpectEntityCount(contexts, 1);
            ExpectBackupCount(contexts, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 0);                                     

            commandBuffer.Enqueue(3, 1, new ICommand[] { new Spawn() }); //Revert to 3

            simulation.Update(1000); //7    
            ExpectEntityCount(contexts, 2);
            ExpectBackupCount(contexts, 3);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 1);

            commandBuffer.Enqueue(4, 1, new Spawn()); //Revert to 4

            simulation.Update(1000); //8      
            ExpectEntityCount(contexts, 3);
            ExpectBackupCount(contexts, 6);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 2);

            commandBuffer.Enqueue(5, 1, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn()); //Revert to 5

            simulation.Update(1000);        
            ExpectEntityCount(contexts, 9);
            ExpectBackupCount(contexts, 15);
            GameEntityCountMatchesActorEntityCount(contexts, 0, 1);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            simulation.Update(1000);
            simulation.Update(1000);
            simulation.Update(1000);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            commandBuffer.Enqueue(6, 1, new MoveSelection(new uint[]{ 0, 3, 5 }, new Vector2(8,3))); 

            simulation.Update(1000);
            ExpectEntityCount(contexts, 10);
            ExpectBackupCount(contexts, 24); 
            GameEntityCountMatchesActorEntityCount(contexts, 0, 2);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            commandBuffer.Enqueue(11, 1, new MoveEntitesOfSpecificActor(contexts.game, 1, new Vector2(3,4)));
            simulation.Update(1000);

            ExpectEntityCount(contexts, 11);
            ExpectBackupCount(contexts, 33); 
            GameEntityCountMatchesActorEntityCount(contexts, 0, 3);
            GameEntityCountMatchesActorEntityCount(contexts, 1, 8);

            TestUtil.TestReplayMatchesHashCode(contexts, simulation.GameLog, _output);
        }

        [Fact]
        public void TestGameLogReplay()
        {
            var contexts = new Contexts();

            var commandBuffer = new CommandQueue();
            var simulation = new Simulation(contexts, commandBuffer, new DefaultViewService());

            simulation.Start(1, 0, new byte[] { 0, 1 });

            simulation.Update(1000); //0                          

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());

            simulation.Update(1000); //1     
            simulation.Update(1000); //2    
            simulation.Update(1000); //3                                                   

            commandBuffer.Enqueue(2, 1, new MoveAll(contexts.game, 1));

            simulation.Update(1000); //4   
            simulation.Update(1000); //5                         

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new MoveEntitesOfSpecificActor(contexts.game, 0, Vector2.Zero));

            simulation.Update(1000); //6                                                    

            commandBuffer.Enqueue(3, 1, new Spawn()); //Revert to 3

            simulation.Update(1000); //7                                                     

            commandBuffer.Enqueue(4, 1, new Spawn()); //Revert to 4

            simulation.Update(1000); //8                                                    

            commandBuffer.Enqueue(5, 1, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn()); //Revert to 5

            simulation.Update(1000);        
            simulation.Update(1000);                            
            simulation.Update(1000);                              
            simulation.Update(1000);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new MoveAll(contexts.game, 0));
            commandBuffer.Enqueue(6, 1, new MoveEntitesOfSpecificActor(contexts.game, 1, Vector2.Zero));

            simulation.Update(1000);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            commandBuffer.Enqueue(11, 1, new MoveEntitesOfSpecificActor(contexts.game, 1, new Vector2(3, 4)));
            simulation.Update(1000);

            ExpectEntityCount(contexts, 12);

            TestUtil.TestReplayMatchesHashCode(contexts, simulation.GameLog, _output);
        }

        [Fact]
        public void TestCommandUsesCorrectEntityIds()
        {
            var contexts = new Contexts();

            var commandBuffer = new CommandQueue();
            var simulation = new Simulation(contexts, commandBuffer, new DefaultViewService());

            simulation.Start(1, 0, new byte[] { 0, 1 });

            simulation.Update(1000);   
            simulation.Update(1000);
            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn());    
            simulation.Update(1000);
            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn());  
            simulation.Update(1000);    

            var selection = new uint[] { 0, 1, 3, 13 };
            var destination = new Vector2(14 , 15);

            commandBuffer.Enqueue(1, 1, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn());
            commandBuffer.Enqueue(8, 1, new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn(), new Spawn());

            commandBuffer.Enqueue(9, 1, new MoveSelection(selection, destination));

            simulation.Update(1000);
            simulation.Update(1000);
            simulation.Update(1000);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new Spawn());
            simulation.Update(1000);

            commandBuffer.Enqueue(contexts.gameState.tick.value, simulation.LocalActorId, new MoveSelection(new uint[]{1,4,2,8,3}, destination));
            simulation.Update(1000);
            simulation.Update(1000);
            simulation.Update(1000);
            simulation.Update(1000);


            contexts.game.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeSubsetOf(Enumerable.Range(0, 18).Select(i => (uint)i));
            contexts.game.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeUnique();

            contexts.game.GetEntities(GameMatcher.LocalId)
                .Where(entity => entity.actorId.value == 1 && selection.Contains(entity.id.value))
                .Select(entity => entity.destination.value).ShouldAllBe(vector2 => vector2.X == destination.X && vector2.Y == destination.Y);

            destination = new Vector2(5, 15);
            commandBuffer.Enqueue(10, 1, new MoveSelection(selection, destination));

            simulation.Update(1000);
            contexts.game.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeSubsetOf(Enumerable.Range(0, 18).Select(i => (uint)i));
            contexts.game.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == 1).Select(entity => entity.id.value).ShouldBeUnique();
            contexts.game.GetEntities(GameMatcher.LocalId)
                .Where(entity => entity.actorId.value == 1 && selection.Contains(entity.id.value))
                .Select(entity => entity.destination.value).ShouldAllBe(vector2 => vector2.X == destination.X && vector2.Y == destination.Y);

            TestUtil.TestReplayMatchesHashCode(contexts, simulation.GameLog, _output);
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

        //    contexts.gameState.tick.value.ShouldBeInRange(9u, 11u);
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

            public ushort Tag { get; }

            public int EntityConfigId;

            public Vector2 Position; 

            public void Execute(InputEntity e)
            {                                   
                e.AddCoordinate(Position);
                e.AddEntityConfigId(EntityConfigId);   
            }

            public void Serialize(Serializer writer)
            {
                throw new NotImplementedException();
            }

            public void Deserialize(Deserializer reader)
            {
                throw new NotImplementedException();
            }
        }

        public class MoveAll : ICommand
        {
            public ushort Tag { get; }

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

            public void Serialize(Serializer writer)
            {
                throw new NotImplementedException();
            }

            public void Deserialize(Deserializer reader)
            {
                throw new NotImplementedException();
            }
        }


        public class MoveEntitesOfSpecificActor : ICommand
        {
            public ushort Tag { get; }

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

            public void Serialize(Serializer writer)
            {
                throw new NotImplementedException();
            }

            public void Deserialize(Deserializer reader)
            {
                throw new NotImplementedException();
            }
        }

        public class MoveSelection : ICommand
        {
            public ushort Tag { get; }

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

            public void Serialize(Serializer writer)
            {
                throw new NotImplementedException();
            }

            public void Deserialize(Deserializer reader)
            {
                throw new NotImplementedException();
            }
        }
    }

}
