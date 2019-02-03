using System;                       
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Lockstep.Client;
using Lockstep.Client.Implementations;
using Lockstep.Core;            
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;     
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Test
{    
    public class GettingSerious 
    {
        private readonly ITestOutputHelper _output;

        public GettingSerious(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(new Converter(output));
        }      
                                                                                                                                                   
        [Fact]                      
        public void TestDump()
        {                       
            var contexts = new Contexts();

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);


            var fileName = "-156373569948";
            var data = ReadFile( $@"E:\Repositories\UnityLockstep\Engine\Test\bin\Debug\netcoreapp2.2\Dumps\{fileName}.txt"); 
            var deserializer = new Deserializer(data);
            var hashCode = deserializer.GetLong();
            var tick = deserializer.GetUInt();
            var localActorId = deserializer.GetByte();
            var allActors = deserializer.GetBytesWithLength();

            IFormatter formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(deserializer.GetRemainingBytes()))
            {
                var dump = (CommandBuffer)formatter.Deserialize(stream);
                foreach (var (tickId, actorCommands) in dump.Buffer)
                {
                    foreach (var (actorId, commands) in actorCommands)
                    {
                        commandBuffer.Insert(tickId, actorId, commands.ToArray());
                    }
                }
            }

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };
            sim.Initialize(new Init { TargetFPS = 1, AllActors = allActors, ActorID = localActorId });

            for (int i = 0; i < tick; i++)
            {
                sim.Update(1000);
            }

            contexts.gameState.hashCode.value.ShouldBe(hashCode);
        }

        public static byte[] ReadFile(string filePath)
        {
            byte[] buffer;
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }
    }   
}
