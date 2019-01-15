using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Framework.Test
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
}