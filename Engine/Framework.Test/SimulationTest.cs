using System;
using System.IO;
using System.Linq;
using System.Text;
using ECS.Data;
using Lockstep.Framework;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Framework.Test
{
    class Converter : TextWriter
    {
        ITestOutputHelper _output;
        public Converter(ITestOutputHelper output)
        {
            _output = output;
        }
        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
        public override void WriteLine(string message)
        {
            _output.WriteLine(message);
        }
        public override void WriteLine(string format, params object[] args)
        {
            _output.WriteLine(format, args);
        }
    }

    public class SimulationTest
    {
        private readonly ITestOutputHelper _output;

        public SimulationTest(ITestOutputHelper output)
        {
            _output = output;
                                
            Console.SetOut(new Converter(output));
        }

        [Fact]
        public void TestSimulate()
        {
            var contexts = Contexts.sharedInstance;

            var ch = new Mock<ICommandHandler>();

            var sim = new Simulation();
            sim.Init(contexts, ch.Object, 0);


            uint ticks = 10;

            for (var i = 0; i < ticks; i++)
            {
                sim.AddFrame(new Frame());
                sim.Simulate();  
            }    
        }
    }
}
