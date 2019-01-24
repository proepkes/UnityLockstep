using System;
using System.Linq;
using BEPUutilities;   
using Lockstep.Client;
using Lockstep.Core;
using Lockstep.Core.Data;
using Lockstep.Core.DefaultServices;
using Lockstep.Core.Interfaces;
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
        public void TestIdGenerator()
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

            new LocalSimulation(new LockstepSystems(new Contexts(), new FrameDataSource())).Execute(command.Object);           

            command.Verify(c => c.Execute(It.IsAny<InputContext>()), Times.Exactly(1));
        }


        [Fact]
        public void TestReadInputReadsFromDataSource()
        {
            var source = new Mock<IFrameDataSource>();

            var readInput = new ReadInput(new Contexts(), source.Object);

            readInput.Execute();

            source.Verify(s => s.GetNext(), Times.Exactly(1));
        }
    }
}
