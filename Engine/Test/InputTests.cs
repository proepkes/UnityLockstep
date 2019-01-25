using System;
using System.Linq; 
using Lockstep.Client;
using Lockstep.Client.Implementations;
using Lockstep.Core;
using Lockstep.Core.Data;             
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
            contexts.SubscribeId();

            var numEntities = 10;

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
            var command = new Mock<ICommand>(); 

            new Simulation(new LockstepSystems(new Contexts()), new LocalDataReceiver()).Execute(command.Object);           

            command.Verify(c => c.Execute(It.IsAny<InputContext>()), Times.Once);
        }
               
    }
}
