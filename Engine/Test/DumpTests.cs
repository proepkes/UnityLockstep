using System;                       
using System.IO;
using System.Linq;
using System.Reflection;      
using Lockstep.Core.Logic;               
using Lockstep.Core.Logic.Serialization.Utils;       
using Lockstep.Game;
using Lockstep.Game.DefaultServices;
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
            TestFileDump("-240158465629");
            TestFileDump("58013408818");
            TestFileDump("179464547357");
        }

        [Fact]
        public void TestDumpRVO()
        {
            TestFileDump(@"RVO\445401195417");
            TestFileDump(@"RVO\526795398181");
        }

        /// <summary>
        /// Runs the simulation twice:
        /// First time including rollbacks as they appeared at runtime
        /// Second time without rollbacks with a pre-filled command-queue
        /// At the end the hashcode of both simulations have to match
        /// </summary>
        /// <param name="fileName"></param>
        private void TestFileDump(string fileName)
        {
            var contexts = new Contexts();   
            var commandBuffer = new CommandQueue();

            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);

            var data = ReadFile($@"{dirPath}\Dumps\{fileName}.bin");
            var deserializer = new Deserializer(data);
            var hashCode = deserializer.GetLong();
            var tick = deserializer.GetUInt();                  
                                                           
            GameLog log;
            using (var stream = new MemoryStream(deserializer.GetRemainingBytes()))
            {
                log = GameLog.ReadFrom(stream);
            }

            var simulation = new Simulation(contexts, commandBuffer, new DefaultViewService());
            simulation.Start(1, log.LocalActorId, log.AllActorIds);

            for (uint i = 0; i < tick; i++)
            {
                if (log.InputLog.ContainsKey(i))
                {
                    var tickCommands = log.InputLog[i];
                    {
                        foreach (var (tickId, allCommands) in tickCommands)
                        {
                            foreach (var (actorId, commands) in allCommands)
                            {
                                commandBuffer.Enqueue(new Input(tickId, actorId, commands.ToArray()));
                            }
                        }
                    }
                }

                simulation.Update(1000);
            }

//            contexts.gameState.hashCode.value.ShouldBe(hashCode);

            TestUtil.TestReplayMatchesHashCode(contexts, simulation.GameLog, _output);
        }


        [Fact]
        public void TestSerializeGameLog()
        {
            var log = new GameLog();
            log.Add(2, 5, 0, new InputParseTest.Spawn());

            using (var stream = new MemoryStream())
            {
                var serializer = new Serializer();
                stream.Write(serializer.Data, 0, serializer.Length);
                log.WriteTo(stream);

                stream.Position = 0;
                var deserializer = new Deserializer(stream.GetBuffer());
                using (var stream2 = new MemoryStream(deserializer.GetRemainingBytes()))
                {
                    var result = GameLog.ReadFrom(stream2).InputLog;
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
