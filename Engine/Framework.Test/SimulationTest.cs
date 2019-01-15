using System.Linq;
using Lockstep.Framework;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Framework.Test
{
    public class SimulationTest
    {
        private readonly ITestOutputHelper _output;

        public SimulationTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestTickSystem()
        {
            var contexts = Contexts.sharedInstance;

            var ch = new Mock<ICommandHandler>();

            var sim = new Simulation();
            sim.Init(contexts, ch.Object, 0);


            var ticks = 10;

            for (var i = 0; i < ticks; i++)
            {                          
                sim.Simulate();  
            }

            contexts.input.tick.value.ShouldBe(ticks);
        }
    }
}
