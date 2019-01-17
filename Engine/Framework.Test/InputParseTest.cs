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
            var inputParser = new DefaultParseInputService();

            var serializer = new NetDataWriter();
            serializer.Put((byte)CommandTag.Spawn);
            new SpawnCommand().Serialize(serializer);


            var command = new SerializedInput { Data = serializer.Data };

            new Simulation(new List<IService> { inputParser, new Mock<IViewService>().Object })
                .Init(0)
                .AddFrame(new Frame { SerializedInputs = new[] { command } })
                .Simulate();

            Contexts.sharedInstance.game.count.ShouldBe(1);
        }

        [Fact]
        public void TestSpawnAndNavigateEntity()
        {
            var inputParser = new DefaultParseInputService();

            var destination = new Vector2(11, 22);

            var serializer = new NetDataWriter();
            serializer.Put((byte)CommandTag.Spawn);
            new SpawnCommand().Serialize(serializer);
            
            //Initialize a new simulation and add a gameentity
            var sim = new Simulation(new List<IService> { inputParser, new Mock<IViewService>().Object })
                .Init(0)
                .AddFrame(new Frame
                {
                    SerializedInputs = new[]
                    {
                        new SerializedInput { Data = serializer.Data }
                    }
                })
                .Simulate();

            //Navigate the new created entity
            var e = Contexts.sharedInstance.game.GetEntities().First();

            serializer.Reset();
            serializer.Put((byte)CommandTag.Navigate);
            new NavigateCommand { Destination = destination, EntityIds = new[] {e.id.value} }.Serialize(serializer);

            sim.AddFrame(new Frame
                {
                    SerializedInputs = new[]
                    {
                        new SerializedInput { Data = serializer.Data }
                    }
                })
                .Simulate();

            Contexts.sharedInstance.input.GetEntities().Length.ShouldBe(0); //There should be no inputs after a simulation was executed

            Contexts.sharedInstance.game.GetGroup(GameMatcher.Destination).GetEntities().Length.ShouldBe(1);

            var gameEntity = Contexts.sharedInstance.game.GetGroup(GameMatcher.Destination).GetSingleEntity();
            gameEntity.destination.value.X.ShouldBe(destination.X);
            gameEntity.destination.value.Y.ShouldBe(destination.Y);
        }
    }
}
