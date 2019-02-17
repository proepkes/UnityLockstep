using System.Linq;
using Entitas;
using FixMath.NET;
using Supercluster.KDTree;

namespace Lockstep.Core.Logic.Systems.Game
{
    public class BuildKdTree : IExecuteSystem
    {
        private readonly GameContext _gameContext;
        private GameEntity[] _entities;

        public BuildKdTree(Contexts contexts)
        {
            _gameContext = contexts.game;
        }       

        public void Execute()
        {
            _entities = _gameContext.GetEntities(GameMatcher.LocalId).OrderBy(e => e.actorId.value).ThenBy(e => e.id.value).ToArray();
            if (_entities.Length > 0)
            {
                var tree = new KDTree<Fix64, GameEntity>(2, _entities.Select(e => new[] { e.position.value.X, e.position.value.Y }).ToArray(), _entities,
                    (x, y) =>
                    {
                        Fix64 dist = 0;
                        for (int i = 0; i < x.Length; i++)
                        {
                            dist += (x[i] - y[i]) * (x[i] - y[i]);
                        }

                        return dist;
                    });

                _gameContext.ReplaceKdTree(tree);

                foreach (var e in _entities)
                {
                    e.neighbors.array = new GameEntity[10];

                    var i = 0;
                    var neighbors = tree.NearestNeighbors(new[] { e.position.value.X, e.position.value.Y }, 10);
                    foreach (var neighbor in neighbors)
                    {
                        e.neighbors.array[i++] = neighbor.Item2;
                    }

                }       
            }
        }
    }
}
