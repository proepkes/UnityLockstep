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

            var fileName = "18_130442405747_log";
            var data = ReadFile( $@"{dirPath}\Dumps\{fileName}.txt"); 
            var deserializer = new Deserializer(data);
            var hashCode = deserializer.GetLong();
            var tick = deserializer.GetUInt();
            var localActorId = deserializer.GetByte();
            var allActors = deserializer.GetBytesWithLength();

            IFormatter formatter = new BinaryFormatter();
            GameLog log;
            using (var stream = new MemoryStream(deserializer.GetRemainingBytes()))
            {
                log = (GameLog)formatter.Deserialize(stream);
            }
                  
            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };
            sim.Initialize(new Init { TargetFPS = 1000, AllActors = allActors, ActorID = localActorId });

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
                                if (actorId == localActorId)
                                {
                                    _output.WriteLine("Local: " + commands.Count + " commands");
                                }
                                commandBuffer.Insert(tickId, actorId, commands.ToArray()); 
                            }
                        }
                    }   
                }
                sim.Update(1);
            }

            contexts.gameState.hashCode.value.ShouldBe(hashCode);
        }


        [Fact]
        public void TestDump2()
        {
            var contexts = new Contexts();

            var systems = new World(contexts, new TestLogger(_output));
            var commandBuffer = new CommandBuffer();

            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);

            var fileName = "-127091875993";
            var data = ReadFile($@"{dirPath}\Dumps\{fileName}.txt");
            var deserializer = new Deserializer(data);
            var hashCode = deserializer.GetLong();
            var tick = deserializer.GetUInt();
            var localActorId = deserializer.GetByte();
            var allActors = deserializer.GetBytesWithLength();

            IFormatter formatter = new BinaryFormatter();
            GameLog log;
            using (var stream = new MemoryStream(deserializer.GetRemainingBytes()))
            {
                log = (GameLog)formatter.Deserialize(stream);
            }

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };
            sim.Initialize(new Init { TargetFPS = 1000, AllActors = allActors, ActorID = localActorId });

            var cs = log.GetAllCommandsForFrame(183);

            for (uint i = 0; i < 300; i++)
            {
                if (log.Log.ContainsKey(i))
                {
                    var tickCommands = log.Log[i];
                    {
                        foreach (var (tickId, allCommands) in tickCommands)
                        {
                            foreach (var (actorId, commands) in allCommands)
                            {
                                if (actorId == localActorId)
                                {
                                    _output.WriteLine("Local: " + commands.Count + " commands");
                                }
                                commandBuffer.Insert(tickId, actorId, commands.ToArray());
                            }
                        }
                    }
                }
                sim.Update(1);
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
