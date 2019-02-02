using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesperateDevs.Utils;
using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems
{
    public class OnNewPredictionCreateBackup : ReactiveSystem<GameStateEntity>
    {
        private readonly Services _services;
        private readonly IGroup<GameEntity> _activeEntities;
        private readonly ActorContext _actorContext;                 
        private readonly GameContext _gameContext;
        private readonly ISnapshotIndexService _snapshotIndexService;
        private GameStateContext _gameStateContext;

        public OnNewPredictionCreateBackup(Contexts contexts, Services services) : base(contexts.gameState)
        {
            _services = services;
            _gameContext = contexts.game;
            _actorContext = contexts.actor;
            _gameStateContext = contexts.gameState;
            _snapshotIndexService = services.Get<ISnapshotIndexService>();

            _activeEntities = contexts.game.GetGroup(GameMatcher.LocalId); 
        }

        protected override ICollector<GameStateEntity> GetTrigger(IContext<GameStateEntity> context)
        {   
            //Create a snapshot as soon as prediction starts
            return context.CreateCollector(GameStateMatcher.Predicting.Added());
        }

        protected override bool Filter(GameStateEntity gameState)
        {
            return gameState.isPredicting;
        }

        protected override void Execute(List<GameStateEntity> entities)
        {                                              
            var currentTick = _gameStateContext.tick.value;


            //Register the tick for which a snapshot is created
            _snapshotIndexService.AddIndex(currentTick);

            var actors = _actorContext.GetEntities(ActorMatcher.Id);
            var gameEnts = _activeEntities.GetEntities();

            foreach (var actor in actors)
            {
                var shadowActor = _actorContext.CreateEntity();

                //LocalId is primary index => don't copy
                foreach (var index in actor.GetComponentIndices().Except(new[] { ActorComponentsLookup.Id }))
                {
                    var component1 = actor.GetComponent(index);
                    var component2 = shadowActor.CreateComponent(index, component1.GetType());
                    component1.CopyPublicMemberValues(component2);
                    shadowActor.AddComponent(index, component2);
                }

                shadowActor.AddBackup(actor.id.value, currentTick); 
            }

            foreach (var e in gameEnts)
            {
                var shadowEntity = _gameContext.CreateEntity();
                                                                                     
                //LocalId is primary index => don't copy; id+actorId should be readonly and stay the same throughout the game
                foreach (var index in e.GetComponentIndices().Except(new[] { GameComponentsLookup.LocalId, GameComponentsLookup.Id, GameComponentsLookup.ActorId }))
                {
                    var component1 = e.GetComponent(index);
                    var component2 = shadowEntity.CreateComponent(index, component1.GetType());
                    component1.CopyPublicMemberValues(component2);
                    shadowEntity.AddComponent(index, component2);
                }

                shadowEntity.AddBackup(e.localId.value, currentTick);    
            }


            _services.Get<ILogService>().Warn("New backup for " + currentTick + "(" + actors.Length + " actors, " + gameEnts.Length + " entities)");
        }
    }
}
