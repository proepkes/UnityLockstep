using System;
using System.Linq;
using BEPUutilities;
using Entitas;
using Lockstep.Client;
using Lockstep.Client.Implementations;
using Lockstep.Client.Interfaces;
using Lockstep.Core;
using Lockstep.Core.Data;
using Lockstep.Core.DefaultServices;
using Lockstep.Core.Systems.GameState;
using Lockstep.Core.Systems.Input;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Test
{    
    public class InputParseTest
    {               
        public InputParseTest(ITestOutputHelper output)
        {
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
        public void TestCommandIsExecuted()
        {
            //var command = new Mock<ISerializableCommand>(); 

            //new Simulation(new GameSystems(new Contexts()), null).Execute(command.Object);           

            //command.Verify(c => c.Execute(It.IsAny<InputContext>()), Times.Once);
        }

        [Fact]
        public void TestStoreEvents()
        {
            //var contexts = new Contexts(); 

            //var systems = new Systems()
            //    .Add(new IncrementTick(contexts))
            //    .Add(new OnSpawnInputCreateEntity(contexts, new DefaultGameService()));

            //systems.Initialize();

            //var input = contexts.input.CreateEntity();
            //input.AddCoordinate(new Vector2());
            //input.AddEntityConfigId(0);

            //systems.Execute();  //Tick 1

            //contexts.events.count.ShouldBe(1);                             
            //contexts.events.GetEntities().First().hasTick.ShouldBeTrue();
            //contexts.events.GetEntities().First().tick.value.ShouldBe<uint>(1);

            //systems.Execute();  //Tick 2    
            //systems.Execute();  //Tick 3  


            //var input2 = contexts.input.CreateEntity();
            //input2.AddCoordinate(new Vector2());
            //input2.AddEntityConfigId(0);

            //systems.Execute();  //Tick 4

            //contexts.events.count.ShouldBe(2); 
        }
    }
}
