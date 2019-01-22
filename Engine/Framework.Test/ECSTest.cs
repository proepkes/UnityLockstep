using System;  
using System.Linq;
using ECS;
using Lockstep.Framework;
using Moq;
using Shouldly;       
using Xunit;
using Xunit.Abstractions;

//TODO: tests currently don't work parallel, maybe shared contexts/entities? 
namespace Framework.Test
{           
    public class ECSTest
    {                                               
        public ECSTest(ITestOutputHelper output)
        {                                      
            Console.SetOut(new Converter(output));
        }      

        [Fact]
        public void TestGameEntityId()
        {
            var contexts = new Contexts();   
                                  
            var container = new ServiceContainer();
            container
                .Register(new Mock<IInputService>().Object);

            new LockstepSystems(contexts, container).Initialize();

            const int numEntities = 10;

            for (uint i = 0; i < numEntities; i++)
            {
                var e = contexts.game.CreateEntity();
                e.hasId.ShouldBeTrue();
            }
            contexts.game.GetEntities().Select(entity => entity.id.value).ShouldBeUnique();
            contexts.game.count.ShouldBe(numEntities);
        }

        [Fact]
        public void TestHashCode()
        {
            var contexts = new Contexts();

            var container = new ServiceContainer();
            container
                .Register(new Mock<IInputService>().Object);

            new LockstepSystems(contexts, container).Initialize();

            const int numEntities = 10;

            for (uint i = 0; i < numEntities; i++)
            {
                var e = contexts.game.CreateEntity(); 
            }                                          

            //TODO: impl.
        }
    }
}
