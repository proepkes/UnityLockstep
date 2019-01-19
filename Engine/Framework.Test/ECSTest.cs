using System;
using System.Collections.Generic;
using System.Linq;
using ECS.Data;
using Lockstep.Framework;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

//TODO: tests currently don't work parallel, probably because of Context.sharedInstance
namespace Framework.Test
{
    public class ECSTest
    {                                               
        public ECSTest(ITestOutputHelper output)
        {                                      
            Console.SetOut(new Converter(output));
        }      

        [Fact]
        public void TestGameEntityId()
        {
            var contexts = Contexts.sharedInstance;     

            new LockstepSystems(contexts, new List<IService>()).Initialize();

            const int numEntities = 10;

            for (uint i = 0; i < numEntities; i++)
            {
                var e = contexts.game.CreateEntity();
                e.hasId.ShouldBeTrue();
            }
            contexts.game.GetEntities().Select(entity => entity.id.value).ShouldBeUnique();
            contexts.game.count.ShouldBe(numEntities);
        }

        [Fact]
        public void TestInputParserGetsCalled()
        {
            var inputParser = new Mock<IParseInputService>();

            var sim = new Simulation(new List<IService> { inputParser.Object });
            sim.Init(0);

            for (var i = 0; i < 10; i++)
            {
                var command = new SerializedInput();
                sim.AddFrame(new Frame { SerializedInputs = new[] { command } });
                sim.Simulate();

                inputParser.Verify(service => service.Parse(It.IsAny<InputContext>(), command), Times.Exactly(1));
            }
        }  
    }
}
