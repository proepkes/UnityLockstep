using System;
using System.Collections.Generic;
using System.Linq;
using BEPUutilities;      
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

        //Tests regarding Rollback currently require to remove the line from Simulation where input gets added to the remoteBuffer. Otherwise the input loops back into the simulation
        [Fact]
        public void TestCreateEntityRollbackLocal()
        {                      
            var contexts = new Contexts();   

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0 };

            sim.Initialize(new Init { TargetFPS = 1});


            sim.Update(1000);     
            sim.Update(1000);                                             
            sim.Update(1000);

            for (int i = 0; i < 10; i++)
            {
                sim.Execute(new SpawnCommand());
            }

            commandBuffer.Insert(2, 1, new ICommand[] { });
                                                                 
            sim.Update(1000);   //3 = 30          
            contexts.game.GetEntities().Count(entity => entity.hasId).ShouldBe(10);

            sim.Update(1000);

            for (int i = 0; i < 10; i++)
            {
                sim.Execute(new SpawnCommand());
            }

            sim.Update(1000);

            contexts.game.GetEntities().Count(entity => entity.hasId).ShouldBe(20);     


            for (int i = 0; i < 10; i++)
            {
                sim.Execute(new SpawnCommand());
            }
            sim.Update(1000);

            contexts.game.GetEntities().Count(entity => entity.hasId).ShouldBe(30);
                                                                  
            _output.WriteLine("Revert to 3");
            commandBuffer.Insert(3, 1, new ICommand[] { });

            sim.Update(1000);

            contexts.game.GetEntities().Count(entity => entity.hasId).ShouldBe(30);

            sim.Update(1000);
            commandBuffer.Insert(4, 1, new ICommand[] { });
            for (int i = 0; i < 10; i++)
            {
                sim.Execute(new SpawnCommand());
            }
            sim.Update(1000);

            contexts.game.GetEntities().Count(entity => entity.hasId).ShouldBe(40);

            sim.Update(1000);
            sim.Update(1000);
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(5, 1, new ICommand[] { new SpawnCommand() });
            }
            sim.Update(1000);


            contexts.game.GetEntities().Count(entity => entity.hasId).ShouldBe(50); 
        }

        [Fact]
        public void TestCreateEntityRollbackRemote()
        {
            
            var contexts = new Contexts();

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0 };

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

            _output.WriteLine("Count: " + contexts.game.count);
            _output.WriteLine("Revert to 3");
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(8, 1, new ICommand[] { new SpawnCommand() });
            }

            sim.Update(1000);
            _output.WriteLine("Count: " + contexts.game.count);
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            sim.Update(1000);
            for (int i = 0; i < 10; i++)
            {
                commandBuffer.Insert(9, 1, new ICommand[] { new SpawnCommand() });
            }
            sim.Update(1000);   

            contexts.game.count.ShouldBe(70);
        }


        public class SpawnCommand : ICommand
        {
            public ushort Tag => 2;

            public int EntityConfigId;

            public Vector2 Position;

            public void Execute(InputEntity e)
            {                                   
                e.AddCoordinate(Position);
                e.AddEntityConfigId(EntityConfigId);   
            }     

        }
    }
}
