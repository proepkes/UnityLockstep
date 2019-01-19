using System;
using System.Collections.Generic;
using System.Linq;
using BEPUutilities;
using ECS.Data;
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

            var serializer = new NetDataWriter();         
            new SpawnCommand
            {
                AssetName = "Test",
                Movable = false
            }.Serialize(serializer);


            var command = new SerializedInput { Data = serializer.Data };

            new Simulation(new List<IService> { inputParser, new Mock<IViewService>().Object })
                .Init(0)
                .AddFrame(new Frame { SerializedInputs = new[] { command } })
                .Simulate();

            Contexts.sharedInstance.game.count.ShouldBe(1);
        }
    }
}
