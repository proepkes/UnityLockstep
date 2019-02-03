using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lockstep.Client;
using Lockstep.Client.Implementations;
using Lockstep.Core;
using Lockstep.Core.Interfaces;
using Lockstep.Network.Messages;
using Newtonsoft.Json;
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

            var sim = new Simulation(systems, commandBuffer) { LagCompensation = 0, SendCommandsToBuffer = false };

            sim.Initialize(new Init { TargetFPS = 1, AllActors = new byte[] { 3 }, ActorID = 3 });

            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);   

            var dump = File.ReadAllText("E:\\Repositories\\UnityLockstep\\Engine\\Test\\bin\\Debug\\netcoreapp2.2/Dumps/-448951665058.txt");

            var bufferDump = JsonConvert.DeserializeObject<Dictionary<uint, Dictionary<byte, List<ICommand>>>>(dump);
            foreach (var (tickId, actorCommands) in bufferDump)
            {
                foreach (var (actorId, commands) in actorCommands)
                {              
                    commandBuffer.Insert(tickId, actorId, commands.ToArray());  
                }
            }

            for (int i = 0; i < 3000; i++)
            {
                sim.Update(1000);
            }

            contexts.gameState.hashCode.value.ShouldBe(-448951665058);
        }
    }
       

}
