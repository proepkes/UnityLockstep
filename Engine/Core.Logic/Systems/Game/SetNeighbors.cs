using System;
using System.Linq;
using Entitas;
using FixMath.NET;
using Supercluster.KDTree;
using Supercluster.KDTree.Utilities;

namespace Lockstep.Core.Logic.Systems.Game
{
    public class SetNeighbors : IExecuteSystem
    {
        private readonly GameContext _gameContext;
        private GameEntity[] _entities;

        public SetNeighbors(Contexts contexts)
        {
            _gameContext = contexts.game;
        }       

        public void Execute()
        {
            _entities = _gameContext.GetEntities(GameMatcher.LocalId).OrderBy(e => e.actorId.value).ThenBy(e => e.id.value).ToArray();
            if (_entities.Length > 0)
            {
                //var neighborList = new BoundedPriorityList<int, Fix64>(10, true);
                //var tree = _gameContext.kdTree.value;
                //foreach (var e in _entities)
                //{
                //    Array.Clear(e.neighbors.array, 0, 10);

                //    neighborList.Clear();
                //    tree.RadialSearch(new[] { e.position.value.X, e.position.value.Y }, 20, neighborList);
                //    var neighbors = neighborList.ToResultSet(tree);

                //    Array.Copy(neighbors.Select(tuple => tuple.Item2).ToArray(), e.neighbors.array, neighbors.Length);
                //}

                var neighborList = new BoundedPriorityList<int, Fix64>(10, true);
                var tree = _gameContext.kdTree.value;
                foreach (var e in _entities)
                {
                    Array.Clear(e.neighbors.array, 0, 10);

                    neighborList.Clear();
                    var n = tree.RadialSearch(new[] { e.position.value.X, e.position.value.Y }, 20, 10);

                    Array.Copy(n.Select(node => node.Value).ToArray(), e.neighbors.array, n.Length);
                }
            }
        }
    }
}
