using System;                       
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Lockstep.Core;
using Lockstep.Core.Commands;
using Lockstep.Core.Services;
using Lockstep.Core.World;
using Lockstep.Game;
using Lockstep.Game.Commands;
using Lockstep.Network.Messages;
using Lockstep.Network.Utils;     
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Test
{    
    public class DumpTests
    {
        private readonly ITestOutputHelper _output;

        public DumpTests(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(new Converter(output));
        }      
                                                                                                                                                   
        [Fact]                      
        public void TestDump()
        {
            TestFileDump("0_3398952201_log");  
        }

        private void TestFileDump(string fileName)
        {
            var contexts = new Contexts();   
            var commandBuffer = new CommandBuffer();

            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);

            var data = ReadFile($@"{dirPath}\Dumps\{fileName}.txt");
            var deserializer = new Deserializer(data);
            var hashCode = deserializer.GetLong();
            var tick = deserializer.GetUInt();
            var localActorId = deserializer.GetByte();
            var allActors = deserializer.GetBytesWithLength();
                                                           
            GameLog log;
            using (var stream = new MemoryStream(deserializer.GetRemainingBytes()))
            {
                log = GameLog.ReadFrom(stream);
            }

            var simulation = new Simulation(contexts, commandBuffer, new TestLogger(_output));
            simulation.Start(1000, localActorId, allActors);

            for (uint i = 0; i < tick; i++)
            {
                if (log.Log.ContainsKey(i))
                {
                    var tickCommands = log.Log[i];
                    {
                        foreach (var (tickId, allCommands) in tickCommands)
                        {
                            foreach (var (actorId, commands) in allCommands)
                            {
                                commandBuffer.Insert(tickId, actorId, commands.ToArray());
                            }
                        }
                    }
                }

                simulation.Update(1);
            }

            contexts.gameState.hashCode.value.ShouldBe(hashCode);

            TestUtil.TestReplayMatchesHashCode(simulation.GameLog, simulation.CurrentTick, hashCode,
                simulation.Services.Get<IDebugService>(), _output);
        }


        [Fact]
        public void TestSerializeGameLog()
        {
            var log = new GameLog();
            log.Add(2, 5, 0, new ICommand[]{ new InputParseTest.Spawn() });

            using (var stream = new MemoryStream())
            {
                var serializer = new Serializer();
                serializer.Put((long)112341);
                serializer.Put((uint)12513);
                serializer.Put((byte)1);
                serializer.PutBytesWithLength(new byte[] { 1 });
                stream.Write(serializer.Data, 0, serializer.Length);
                log.WriteTo(stream);

                stream.Position = 0;
                var deserializer = new Deserializer(stream.GetBuffer());
                var hashCode = deserializer.GetLong();
                var tick = deserializer.GetUInt();
                var localActorId = deserializer.GetByte();
                var allActors = deserializer.GetBytesWithLength();

                using (var stream2 = new MemoryStream(deserializer.GetRemainingBytes()))
                {
                    var result = GameLog.ReadFrom(stream2).Log;
                    result.Keys.Any(u => u != 2).ShouldBeFalse();
                }
            }
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
