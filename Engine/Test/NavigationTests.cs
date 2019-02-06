using System;        
using BEPUutilities;
using Lockstep.Core.Services;
using Lockstep.Core.Systems.Navigation;
using Moq;           
using Xunit;
using Xunit.Abstractions;

namespace Test
{                                                                                 
    public class NavigationTests
    {          
        private readonly ITestOutputHelper _output;

        public NavigationTests(ITestOutputHelper output)
        {                                    
            _output = output;
            Console.SetOut(new Converter(output));   
        }           

        [Fact]
        public void TestNavigableEntityGetsAddedToNavigationService()
        {
            var service = new Mock<INavigationService>();
            var contexts = new Contexts();       


            var e = contexts.game.CreateEntity();
            e.AddPosition(Vector2.Zero);

            var system = new OnNavigableDoRegisterAgent(contexts, service.Object);

            system.Execute();

            service.Verify(s => s.AddAgent(It.IsAny<uint>(), It.IsAny<Vector2>()), Times.Never);

            e.isNavigable = true;
            system.Execute();

            service.Verify(s => s.AddAgent(It.IsAny<uint>(), It.IsAny<Vector2>()), Times.Once);
        }
    }
}
