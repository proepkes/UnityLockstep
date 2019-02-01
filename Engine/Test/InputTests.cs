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
                sim.Execute(new SpawnCommand());
            }

            sim.Update(1000); //1            
            systems.CurrentTick.ShouldBe((uint)2);
            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 1);

            sim.Update(1000); //2             
            systems.CurrentTick.ShouldBe((uint)3);
            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 1);

            commandBuffer.Insert(1, 1, new ICommand[] { });
                                                                 
            sim.Update(1000); //3                
            systems.CurrentTick.ShouldBe((uint)4);
            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 1);   


            commandBuffer.Insert(2, 1, new ICommand[] { new MoveCommand(contexts.game), });


            sim.Update(1000); //4                
            systems.CurrentTick.ShouldBe((uint)5);
            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 2);

            for (int i = 0; i < 1; i++)
            {
                sim.Execute(new SpawnCommand());
            }

            sim.Update(1000); //5        
            systems.CurrentTick.ShouldBe((uint)6);

            ExpectEntityCount(contexts, 2);
            ExpectShadowCount(contexts, 3);


            for (int i = 0; i < 1; i++)
            {
                sim.Execute(new SpawnCommand());
            }

            sim.Update(1000); //6
            ExpectEntityCount(contexts, 3);
            ExpectShadowCount(contexts, 4);
                                                  
            commandBuffer.Insert(3, 1, new ICommand[] { }); //Revert to 3

            sim.Update(1000);
            ExpectEntityCount(contexts, 3);
            ExpectShadowCount(contexts, 4); 
                                                                                               
            sim.Update(1000);
            commandBuffer.Insert(4, 1, new ICommand[] { });
            for (int i = 0; i < 1; i++)
            {
                sim.Execute(new SpawnCommand());
            }
            sim.Update(1000);
            ExpectEntityCount(contexts, 4);
            ExpectShadowCount(contexts, 5);

            sim.Update(1000);
            sim.Update(1000);
            for (int i = 0; i < 1; i++)
            {
                commandBuffer.Insert(5, 1, new ICommand[] { new SpawnCommand() });
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

            sim.Initialize(new Init { TargetFPS = 1 });

            sim.Update(1000);
            for (int i = 0; i < 10; i++)
            {
                sim.Execute(new SpawnCommand());
            }
            sim.Update(1000); 
            sim.Update(1000);
            sim.Update(1000);      
            sim.Update(1000);   

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(2, 1, new ICommand[] { new SpawnCommand() });
            }              

            sim.Update(1000);                     

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(4, 1, new ICommand[] { new SpawnCommand() });
            }

                                       
            for (int i = 0; i < 100; i++)
            {
                sim.Update(1000);
            }                          

            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(5, 1, new ICommand[] { new SpawnCommand() });
            }

            sim.Update(1000);    
            sim.Update(1000);
            sim.Update(1000);


            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(7, 1, new ICommand[] { new SpawnCommand() });
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
                commandBuffer.Insert(8, 1, new ICommand[] { new SpawnCommand() });
            }

            sim.Update(1000);                                   
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(9, 1, new ICommand[] { new SpawnCommand() });
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

            sim.Initialize(new Init { TargetFPS = 1 });

            uint frameCounter = 0;

            systems.CurrentTick.ShouldBe(frameCounter++);

            sim.Update(1000); //0                           
            systems.CurrentTick.ShouldBe(frameCounter++);

            sim.Execute(new SpawnCommand());

            sim.Update(1000); //1                           
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 1);

            sim.Update(1000); //2                             
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 1);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 0).ShouldBe(1);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 1).ShouldBe(0);

            commandBuffer.Insert(1, 1, new ICommand[] { });

            sim.Update(1000); //3                            
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 1);


            commandBuffer.Insert(2, 1, new ICommand[] { new MoveCommand(contexts.game), });


            sim.Update(1000); //4                           
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 2);

           
            sim.Update(1000); //5                           
            systems.CurrentTick.ShouldBe(frameCounter++);

            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 2);

            sim.Execute(new MoveSpecificCommand(contexts.game, 0));

            sim.Update(1000); //6                          
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 1);
            ExpectShadowCount(contexts, 3);
            systems.Services.Get<IPlayerEntityIdProvider>().Get(0).ShouldBe((uint)1);
            systems.Services.Get<IPlayerEntityIdProvider>().Get(1).ShouldBe((uint)0);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 0).ShouldBe(1);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 1).ShouldBe(0);

            commandBuffer.Insert(3, 1, new ICommand[] { new SpawnCommand() }); //Revert to 3

            sim.Update(1000); //7                          
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 2);
            ExpectShadowCount(contexts, 4);
            systems.Services.Get<IPlayerEntityIdProvider>().Get(0).ShouldBe((uint)1);
            systems.Services.Get<IPlayerEntityIdProvider>().Get(1).ShouldBe((uint)1);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 0).ShouldBe(1);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 1).ShouldBe(1);

            commandBuffer.Insert(4, 1, new ICommand[] { new SpawnCommand() }); //Revert to 4

            sim.Update(1000); //8                          
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 3);
            ExpectShadowCount(contexts, 5);
            systems.Services.Get<IPlayerEntityIdProvider>().Get(0).ShouldBe((uint)1);
            systems.Services.Get<IPlayerEntityIdProvider>().Get(1).ShouldBe((uint)2);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 0).ShouldBe(1);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 1).ShouldBe(2);

            commandBuffer.Insert(5, 1, new ICommand[] { new SpawnCommand(), new SpawnCommand(), new SpawnCommand(), new SpawnCommand(), new SpawnCommand(), new SpawnCommand() }); //Revert to 4

            sim.Update(1000);                           
            systems.CurrentTick.ShouldBe(frameCounter++);
            ExpectEntityCount(contexts, 9);
            ExpectShadowCount(contexts, 11);
            systems.Services.Get<IPlayerEntityIdProvider>().Get(0).ShouldBe((uint)1);
            systems.Services.Get<IPlayerEntityIdProvider>().Get(1).ShouldBe((uint)8);

            sim.Update(1000);
            systems.CurrentTick.ShouldBe(frameCounter++);
            sim.Update(1000);
            systems.CurrentTick.ShouldBe(frameCounter++);
            sim.Update(1000);
            systems.CurrentTick.ShouldBe(frameCounter++);   

            sim.Execute(new SpawnCommand());
            commandBuffer.Insert(6, 1, new ICommand[] { new MoveSpecificCommand(contexts.game, 1),  }); 

            sim.Update(1000);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 0).ShouldBe(2);
            contexts.game.GetEntities().Count(entity => !entity.isShadow && entity.ownerId.value == 1).ShouldBe(8);
            ExpectEntityCount(contexts, 10);
            ExpectShadowCount(contexts, 20); //Last frame 11 shadows + 1 newShadow from player 0 + 8 move-shadows from player 1     
        }

        [Fact]
        public void TestThread()
        {
            var contexts = new Contexts();

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };

            sim.Initialize(new Init { TargetFPS = 10 });             
            sim.StartAsThread();

            Thread.Sleep(1000);

            systems.CurrentTick.ShouldBeInRange(9u, 11u);
        }

        private void ExpectEntityCount(Contexts contexts, int value)
        {
            contexts.game.GetEntities().Count(entity => !entity.isShadow).ShouldBe(value);

        }

        private void ExpectShadowCount(Contexts contexts, int value)
        {      
            contexts.game.GetEntities().Count(entity => entity.isShadow).ShouldBe(value);
        }


        public class SpawnCommand : ICommand
        {                             
            public int EntityConfigId;

            public Vector2 Position;

            public void Execute(InputEntity e)
            {                                   
                e.AddCoordinate(Position);
                e.AddEntityConfigId(EntityConfigId);   
            }     

        }
        public class MoveCommand : ICommand
        {  

            private GameContext contexts;

            public MoveCommand(GameContext contexts)
            {
                this.contexts = contexts;
            }

            public void Execute(InputEntity e)
            {
                foreach (var gameEntity in contexts.GetEntities().Where(entity => !entity.isShadow))
                {
                    gameEntity.ReplacePosition(new Vector2(2, 2));
                }
            }

        }

        public class MoveSpecificCommand : ICommand
        {                                    
            private GameContext contexts;
            private readonly byte _commanderid;

            public MoveSpecificCommand(GameContext contexts, byte commanderid)
            {
                this.contexts = contexts;
                _commanderid = commanderid;
            }

            public void Execute(InputEntity e)
            {
                foreach (var gameEntity in contexts.GetEntities().Where(entity => !entity.isShadow && entity.ownerId.value == _commanderid))
                {
                    gameEntity.ReplacePosition(new Vector2(2, 2));
                }
            }

        }
    }
}
