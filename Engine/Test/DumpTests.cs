using System;                       
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Lockstep.Client;
using Lockstep.Client.Commands;
using Lockstep.Client.Implementations;
using Lockstep.Core;
using Lockstep.Core.Interfaces;
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
        public void TestDump1()
        {                      
            TestDump("2_-535026177646_log");  
        }

        [Fact]
        public void TestDump2()
        {
            TestDump("37_-546443594864_log");
        }

        private void TestDump(string fileName)
        {
            var contexts = new Contexts();   
            var systems = new World(contexts, new TestLogger(_output));
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

            IFormatter formatter = new BinaryFormatter();
            GameLog log;
            using (var stream = new MemoryStream(deserializer.GetRemainingBytes()))
            {
                log = (GameLog) formatter.Deserialize(stream);
            }

            var sim = new Simulation(systems, commandBuffer) {LagCompensation = 0, SendCommandsToBuffer = false};
            sim.Initialize(new Init {TargetFPS = 1000, AllActors = allActors, ActorID = localActorId});

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

                                    systems.AddInput(tickId, actorId, commands);
                                }
                                else
                                {
                                    commandBuffer.Insert(tickId, actorId, commands.ToArray());
                                } 
                            }
                        }
                    }
                }

                sim.Update(1);
            }

            contexts.gameState.hashCode.value.ShouldBe(hashCode);
            commandBuffer.Buffer.ShouldBeEmpty();


            contexts.Reset();
            var debug = systems.Services.Get<IDebugService>();
            systems = new World(contexts, new TestLogger(_output));
            sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };
            sim.Initialize(new Init { TargetFPS = 1000, AllActors = allActors, ActorID = localActorId });

            foreach (var (occurTickId, tickCommands) in log.Log)
            {
                foreach (var (tickId, allCommands) in tickCommands)
                {
                    foreach (var (actorId, commands) in allCommands)
                    {
                        if (commands.Any(command => command is NavigateCommand))
                        {                        
                        }
                        if (actorId == localActorId)
                        {
                            _output.WriteLine("Local: " + commands.Count + " commands");

                            systems.AddInput(tickId, actorId, commands);
                        }
                        else
                        {
                            commandBuffer.Insert(tickId, actorId, commands.ToArray());
                        }
                    }
                }
            }   

            var debug2 = systems.Services.Get<IDebugService>();
            debug.ShouldNotBeSameAs(debug2);                                                              

            for (uint i = 0; i < tick; i++)
            {
                sim.Update(1);
                if (debug.HasHash(systems.CurrentTick))
                {
                    debug.GetHash(systems.CurrentTick).ShouldBe(contexts.gameState.hashCode.value);
                }
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
