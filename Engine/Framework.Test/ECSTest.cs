using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

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
            var contexts = Contexts.sharedInstance;     

            new LockstepSystems(contexts, new ExternalServices(null, null, null)).Initialize();

            uint ticks = 10;

            for (uint i = 0; i < ticks; i++)
            {
                var e = contexts.game.CreateEntity();
                e.hasId.ShouldBeTrue();
                e.id.value.ShouldNotBeOneOf(contexts.game.GetEntities().Where(entity => entity != e).Select(entity => entity.id.value).ToArray());
            }
        }
    }
}
