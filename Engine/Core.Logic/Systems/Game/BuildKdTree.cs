using System.Linq;
using BEPUutilities;
using Entitas;
using FixMath.NET;
using Lockstep.Common.Logging;
using Lockstep.Core.State.KdTree;
using Lockstep.Core.State.KdTree.Math;

namespace Lockstep.Core.Logic.Systems.Game
{
    public class BuildKdTree : IExecuteSystem
    {
        private readonly Contexts _contexts;
        private readonly GameContext _gameContext;
        private GameEntity[] _entities;

        public BuildKdTree(Contexts contexts)
        {
            _contexts = contexts;
            _gameContext = contexts.game;
        }

        public void Execute()
        {
            _entities = _gameContext.GetEntities(GameMatcher.LocalId).OrderBy(e => e.actorId.value).ThenBy(e => e.id.value).ToArray();
            if (_entities.Length > 0)
            {                
                var tree = new KdTree<Fix64, GameEntity>(2, new Fix64Math());
                foreach (var e in _entities)
                {
                    tree.Add(new []{ e.position.value.X, e.position.value.Y }, e);
                }
                foreach (var e in _entities)
                {
                    int k = 0;
                    var neighbors = tree.GetNearestNeighbours(new[] {e.position.value.X, e.position.value.Y}, 10);
                    e.neighbors.neighborsECS = new GameEntity[10];
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.Value != e)
                        {                 
                            e.neighbors.neighborsECS[k++] = neighbor.Value;
                        }
                    }
                }
            }
        }
    }
}
