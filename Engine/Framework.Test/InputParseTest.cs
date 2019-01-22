using System;
using ECS;
using ECS.Data;
using Framework.Test.LiteNetLib;
using Lockstep.Framework;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Services;
using Moq;
using Shouldly;        
using Xunit;
using Xunit.Abstractions;

namespace Framework.Test
{    
    public class InputParseTest
    {
        public InputParseTest(ITestOutputHelper output)
        {
            Console.SetOut(new Converter(output));
        }

        [Fact]
        public void TestSpawnInputCreatesEntity()
        {

            var contexts = new Contexts();
            var inputParser = new ParseInputService(new LiteNetLibNetworkReader());

            var container = new ServiceContainer()
                .Register<IParseInputService>(inputParser);;

            var serializer = new LiteNetLibNetworkWriter();         
            new SpawnCommand().Serialize(serializer);


            var command = new SerializedInput { Data = serializer.Data };

            new Simulation(contexts, container)
                .Init(0)
                .AddFrame(new Frame { SerializedInputs = new[] { command } })
                .Simulate();

            contexts.game.count.ShouldBe(1);
        }

        [Fact]
        public void TestInputParserGetsCalled()
        {
            var inputParser = new Mock<IParseInputService>();
            var container = new ServiceContainer();
            container.Register(inputParser.Object);

            var sim = new Simulation(new Contexts(), container);
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
