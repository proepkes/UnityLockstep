using System;      
using ECS.Data;
using ECS.Features;
using LiteNetLib.Utils;
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
            var inputParser = new ParseInputService();

            var container = new ServiceContainer();
            container.Register<IParseInputService>(inputParser);
            container.Register(new Mock<IViewService>().Object);

            var serializer = new NetDataWriter();         
            new SpawnCommand
            {
                AssetName = "Test",
                Movable = false
            }.Serialize(serializer);


            var command = new SerializedInput { Data = serializer.Data };

            new Simulation(container)
                .Init(0)
                .AddFrame(new Frame { SerializedInputs = new[] { command } })
                .Simulate();

            Contexts.sharedInstance.game.count.ShouldBe(1);
        }
    }
}
