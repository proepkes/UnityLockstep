using System;
using Lockstep.Framework;
using Moq;
using Xunit;

namespace Framework.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var ch = new Mock<ICommandHandler>();

            var sim = new Simulation();
            sim.Init(Contexts.sharedInstance, ch.Object, 0);

            for (int i = 0; i < 10; i++)
            {
                sim.Simulate();  
            }
        }
    }
}
