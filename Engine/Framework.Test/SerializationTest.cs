using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.Test.LiteNetLib;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Networking.Messages;
using Lockstep.Framework.Networking.Serialization;
using Shouldly;
using Xunit;

namespace Framework.Test
{
    public class SerializationTest
    {                                             
        [Fact]
        public void TestSerialzation()
        {
            //Clientside create command, send to server
            var writer = new LiteNetLibSerializer();
            new SpawnCommand().Serialize(writer);

            //Serverside, gather input
            var writer2 = new LiteNetLibSerializer();
            var packer = new InputPacker();
            packer.AddInput(writer.Data);
            packer.Pack(writer2);

            //Clientside, receive input
            var reader = new LiteNetLibDeserializer();
            reader.SetSource(writer2.Data);

            var messageTag = reader.GetByte();
                messageTag.ShouldBe((byte)MessageTag.Frame);

            var frame = new InputParser(
                r =>
                {
                    var tag = (CommandTag)r.PeekUShort();
                    switch (tag)
                    {
                        case CommandTag.Spawn:
                            return new SpawnCommand();
                        default:
                            return null;
                    }
                }).DeserializeInput(reader);  
            
            frame.ShouldNotBeNull();
            frame.Commands.Length.ShouldBe(1);
            frame.Commands.First().ShouldBeOfType<SpawnCommand>();
        }
    }
}
