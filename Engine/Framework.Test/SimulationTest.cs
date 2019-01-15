using System;
using System.IO;     
using System.Text;
using ECS.Data;
using FixMath.NET;
using Lockstep.Framework;                      
using Moq;        
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
        public void TestCommandService()
        {
            var contexts = Contexts.sharedInstance;

            var commandService = new Mock<ICommandService>();
            var timeService = new Mock<ITimeService>();                                                                

            var sim = new Simulation();
            sim.Init(contexts, commandService.Object, timeService.Object, 0);


            uint ticks = 10;

            for (uint i = 0; i < ticks; i++)
            {
                var command = new Command();
                sim.AddFrame(new Frame { Commands = new[]{ command } });
                sim.Simulate();

                commandService.Verify(service => service.Process(contexts.input, command), Times.Exactly(1));
            } 
        }

        [Fact]
        public void TestTimeService()
        {
            var contexts = Contexts.sharedInstance;

            var commandService = new Mock<ICommandService>();
            var timeService = new Mock<ITimeService>();
            timeService.Setup(service => service.FixedDeltaTime).Returns(() => Fix64.One / 20);


            var sim = new Simulation();
            sim.Init(contexts, commandService.Object, timeService.Object, 0);


            uint ticks = 10;

            for (uint i = 0; i < ticks; i++)
            {
                var command = new Command();
                sim.AddFrame(new Frame { Commands = new[] { command } });
                sim.Simulate();            
            }
        }
    }
}
