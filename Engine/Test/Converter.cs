using System.IO;
using System.Text;
using Lockstep.Common.Logging;      
using Xunit.Abstractions;

namespace Test
{
    class Converter : TextWriter
    {
        readonly ITestOutputHelper _output;
        public Converter(ITestOutputHelper output)
        {
            _output = output;

            Log.SetLogAllSeverities();
            Log.OnMessage += (sender, args) => _output.WriteLine(args.LogSeverity + ": " + args.Message);
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
}