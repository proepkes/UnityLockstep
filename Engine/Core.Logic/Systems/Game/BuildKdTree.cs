//using System.Linq;
//using System.Threading.Tasks;
//using Entitas;
//using FixMath.NET;
//using Lockstep.Core.State.Game.KdTree;
//using Lockstep.Core.State.Game.KdTree.Math;

//namespace Lockstep.Core.Logic.Systems.Game
//{
//    public class BuildKdTree : IExecuteSystem
//    {
//        private readonly GameContext _gameContext;
//        private GameEntity[] _entities;

//        public BuildKdTree(Contexts contexts)
//        {
//            _gameContext = contexts.game;
//        }       

//        public void Execute()
//        {
//            _entities = _gameContext.GetEntities(GameMatcher.LocalId).OrderBy(e => e.actorId.value).ThenBy(e => e.id.value).ToArray();
//            if (_entities.Length > 0)
//            {
//                var tree = new KdTree<Fix64, GameEntity>(2, new Fix64Math());
//                foreach (var e in _entities)
//                {
//                    tree.Add(new []{ e.position.value.X, e.position.value.Y }, e);
//                }

//                _gameContext.ReplaceKdTree(tree);

//                foreach (var e in _entities)
//                {
//                    e.neighbors.array = new GameEntity[10];

//                    var i = 0;
//                    var neighbors = tree.GetNearestNeighbors(new[] { e.position.value.X, e.position.value.Y }, 10);
//                    foreach (var neighbor in neighbors)
//                    {
//                        e.neighbors.array[i++] = neighbor.Value;
//                    }

//                }       
//            }
//        }
//    }
//}
