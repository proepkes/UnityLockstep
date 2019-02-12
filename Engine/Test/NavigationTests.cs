using System;        
using BEPUutilities;
using Lockstep.Core.Logic;
using Lockstep.Core.Logic.Interfaces.Services;
using Lockstep.Core.Logic.Systems.Game.Navigation;    
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


            var services = new ServiceContainer();
            services.Register(service.Object);

            var e = contexts.game.CreateEntity();
            e.AddPosition(Vector2.Zero);
            e.AddLocalId(0);
            e.isNew = true;

            var system = new OnNavigableDoRegisterAgent(contexts, services);

            system.Execute();

            service.Verify(s => s.AddAgent(It.IsAny<uint>(), It.IsAny<Vector2>()), Times.Never);

            var e2 = contexts.game.CreateEntity();
            e2.AddPosition(Vector2.Zero);
            e2.AddLocalId(1);
            e2.isNavigable = true;
            e2.isNew = true;

            system.Execute();

            service.Verify(s => s.AddAgent(It.IsAny<uint>(), It.IsAny<Vector2>()), Times.Once);
        }
    }
}
