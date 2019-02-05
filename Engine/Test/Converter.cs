using System.IO;
using System.Text;
using Lockstep.Game.Services;
using Xunit.Abstractions;

namespace Test
{
    class Converter : TextWriter
    {
        readonly ITestOutputHelper _output;
        public Converter(ITestOutputHelper output)
        {
            _output = output;
        }

        public override Encoding Encoding => Encoding.Default;

        public override void WriteLine(string message)
        {
            _output.WriteLine(message);
        }
        public override void WriteLine(string format, params object[] args)
        {
            _output.WriteLine(format, args);
        }
    }

    class TestLogger : ILogService
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestLogger(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }
        public void Warn(object message)
        {
            _outputHelper.WriteLine($"Warn: {message}");
        }

        public void Trace(object message)
        {
            _outputHelper.WriteLine($"Trace: {message}");
        }
    }
}