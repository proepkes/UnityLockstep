using Lockstep.Core.Data;     
using Xunit;

namespace Framework.Test
{
    public class SerializationTest
    {
        class TestCommand : ICommand
        {    

            public void Execute(InputContext context)
            {                                         
            }
        }

        [Fact]
        public void TestSerialzation()
        {
            ////Clientside create command, send to server
            //var writer = new LiteNetLibSerializer();
            //new TestCommand();

            ////Serverside, gather input
            //var writer2 = new LiteNetLibSerializer();
            //var packer = new InputPacker();
            //packer.AddInput(writer.Data);
            //packer.Pack(writer2);

            ////Clientside, receive input
            //var reader = new LiteNetLibDeserializer();
            //reader.SetSource(writer2.Data);

            //var messageTag = reader.GetByte();
            //messageTag.ShouldBe((byte)MessageTag.Frame);

            //var commands = new InputParser(r => new TestCommand()).DeserializeInput(reader);

            //commands.ShouldNotBeNull();
            //commands.Length.ShouldBe(1);
            //commands.First().ShouldBeOfType<TestCommand>();
        }
    }
}
