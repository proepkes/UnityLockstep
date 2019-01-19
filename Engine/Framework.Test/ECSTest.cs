using System;  
using System.Linq;
using ECS.Data;
using ECS.Features;
using Lockstep.Framework;
using Moq;
using Shouldly;       
using Xunit;
using Xunit.Abstractions;

//TODO: tests currently don't work parallel, probably because of Context.sharedInstance
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
                .Register(new Mock<IParseInputService>().Object)
                .Register(new Mock<IViewService>().Object);

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
    }
}
