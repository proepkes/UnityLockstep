using System;
using System.Linq;
using BEPUutilities;     
using Lockstep.Client;
using Lockstep.Core;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Test
{                                                                                 
    public class PathfindingTest
    {                  
        class SpawnCommand : ICommand
        {
            public void Execute(InputContext context)
            {
                context.CreateEntity().AddSpawnInputData(0, 0, Vector2.Zero);
            }
        }

        class NavigateCommand : ICommand
        {
            private readonly int _entityId;
            private readonly Vector2 _destination;

            public NavigateCommand(int entityId, Vector2 destination)
            {
                _entityId = entityId;
                _destination = destination;
            }

            public void Execute(InputContext context)
            {
                context.CreateEntity().AddNavigationInputData(new []{_entityId}, _destination);
            }
        }

        class MakeEveryEntityNavigable : IGameService
        {
            public void LoadEntity(GameEntity entity, int configId)
            {
                //GameService adds additional components depending on configId
                entity.isNavigable = true;
            }
        }

        private readonly ITestOutputHelper _output;

        public PathfindingTest(ITestOutputHelper output)
        {                                    
            _output = output;
            Console.SetOut(new Converter(output));
        }  


        [Fact]
        public void TestSimpleNavigationService()
        {
            var contexts = new Contexts();
            var destination = new Vector2(111, 22);          

            //Initialize a new simulation and add a gameentity by adding a spawncommand to the input 
            var sim = new LocalSimulation(new LockstepSystems(contexts, new FrameDataSource(), new MakeEveryEntityNavigable()));
            sim.Execute(new SpawnCommand());

            var e = contexts.game.GetEntities().First();
            var before = e.position.value;

            sim.Execute(new NavigateCommand(e.id.value, destination));

            for (int i = 0; i < 500; i++)
            {
                sim.Systems.DataSource.Insert(new Frame());
                sim.Systems.Tick();
            }

            e.position.value.X.ShouldNotBe(before.X);
            e.position.value.Y.ShouldNotBe(before.Y);   
        }  
    }
}
