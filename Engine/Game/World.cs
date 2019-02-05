using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Entitas;
using Lockstep.Core.Services;
using Lockstep.Game.Commands;
using Lockstep.Game.Features;
using Lockstep.Game.Services;
using Lockstep.Game.Services.Defaults;
using Lockstep.Game.Services.Defaults.Navigation;
using Lockstep.Game.Systems;
using Lockstep.Game.Systems.Debugging;
using Lockstep.Game.Systems.GameState;
using Lockstep.Network.Messages;


namespace Lockstep.Game
{
    public class World : Feature
    {

        /// <summary>
        /// Amount of ticks until a command gets executed locally
        /// </summary>
        public uint LagCompensation { get; set; }

        /// <summary>
        /// If true, executed commands will be distributed to other actors (default). Mainly for testing because in a test-context the executed commands would loop back into the buffer.
        /// </summary>
        public bool SendCommandsToBuffer { get; set; } = true;

        /// <summary>
        /// The local actor id received from the server.
        /// </summary>
        public byte LocalActorId { get; private set; }

        /// <summary>
        /// Stores all inputs including the tick in which the input was added. Can be used to exactly re-simulate a game (including rollback/prediction)
        /// </summary>
        public GameLog GameLog { get; } = new GameLog();

        public bool Running { get; private set; }

        public Contexts Contexts { get; }

        public ServiceContainer Services { get; }

        public uint CurrentTick => Contexts.gameState.tick.value;

        public int EntitiesInCurrentTick => Contexts.game.GetEntities(GameMatcher.LocalId).Length;

        private float _tickDt;
        private float _accumulatedTime;

        private readonly GameContext _gameContext;
        private readonly ActorContext _actorContext;

        private readonly IViewService _view;
        private readonly ILogService _logger;

        private readonly ICommandBuffer _remoteCommandBuffer;
        private readonly List<ICommand> _commandCache = new List<ICommand>();

        public World(Contexts contexts, ICommandBuffer remoteCommandBuffer,  params IService[] additionalServices)
        {
            Contexts = contexts;

            Services = new ServiceContainer();
            foreach (var service in additionalServices)
            {
                Services.TryRegister(service);
            }
            RegisterDefaultServices();
            
            _view = Services.Get<IViewService>();
            _logger = Services.Get<ILogService>();

            _gameContext = contexts.game;
            _actorContext = contexts.actor;
            _remoteCommandBuffer = remoteCommandBuffer;
        }

        protected virtual void AddFeatures()
        {
            Add(new CalculateHashCode(Contexts, Services));

            Add(new OnNewPredictionCreateBackup(Contexts, Services));    

            Add(new InputFeature(Contexts, Services));

            Add(new VerifySelectionIdExists(Contexts, Services));

            Add(new NavigationFeature(Contexts, Services));

            Add(new GameEventSystems(Contexts));

            Add(new CalculateHashCode(Contexts, Services));

            Add(new RemoveNewFlag(Contexts));

            Add(new IncrementTick(Contexts));

            Add(new VerifyNoDuplicateBackups(Contexts, Services));
        }

        private void RegisterDefaultServices()
        {
            Services.TryRegister(new DefaultHashService());
            Services.TryRegister(new DefaultViewService());
            Services.TryRegister(new DefaultNavigationService());
            Services.TryRegister(new DefaultSnapshotIndexService());
            Services.TryRegister(new DefaultDebugService());
            Services.TryRegister(new DefaultLogService());
        }

        public void Initialize(Init init)
        {
            AddFeatures();

            Initialize();

            _tickDt = 1000f / init.TargetFPS;
            LocalActorId = init.ActorID;

            foreach (var actorId in init.AllActors)
            {
                var actor = Contexts.actor.CreateEntity();
                actor.AddId(actorId);
                actor.AddEntityCount(0);
            }

            Running = true;
        }

        public void AddInput(uint tickId, byte actor, List<ICommand> input)
        {
            GameLog.Add(CurrentTick, tickId, actor, input.ToArray());

            foreach (var command in input)
            {
                var inputEntity = Contexts.input.CreateEntity();
                command.Execute(inputEntity);

                inputEntity.AddTick(tickId);
                inputEntity.AddActorId(actor);
            }

            //TODO: after adding input, order it by timestamp => if commands intersect, the first one should win, timestamp should be added by server, RTT has to be considered
            //ordering by timestamp requires loopback functionality because we have to wait for server-response; at the moment commands get distributed to all clients except oneself
            //if a command comes back from server and it was our own command, the local command has to be overwritten instead of just adding it (as it is at the moment)
        }

        /// <summary>
        /// Executes the command on CurrentTick + LagCompensation as the local actor
        /// </summary>
        /// <param name="command"></param>
        public void Execute(ICommand command)
        {
            if (!Running)
            {
                return;
            }

            lock (_commandCache)
            {
                _commandCache.Add(command);
            }
        }

        public void Update(float elapsedMilliseconds)
        {
            if (!Running)
            {
                return;
            }

            SyncCommandBuffer();

            _accumulatedTime += elapsedMilliseconds;

            while (_accumulatedTime >= _tickDt)
            {
                Tick();

                _accumulatedTime -= _tickDt;
            }
        }



        /// <summary>
        /// Reverts all changes that were done during or after the given tick
        /// </summary>
        public void RevertToTick(uint tick)
        {
            _logger.Trace("Rollback to " + tick);

            //Get the actual tick that we have a snapshot for
            var resultTick = Services.Get<ISnapshotIndexService>().GetFirstIndexBefore(tick);

            /*
             * ====================== Revert actors ======================
             * most importantly: the entity-count per actor gets reverted so the composite key (Id + ActorId) of GameEntities stays in sync
             */

            var backedUpActors = _actorContext.GetEntities(ActorMatcher.Backup).Where(e => e.backup.tick == resultTick);
            foreach (var backedUpActor in backedUpActors)
            {
                backedUpActor.CopyTo(
                    _actorContext.GetEntityWithId(backedUpActor.backup.actorId),                                   //Current Actor
                    true,                                                                             //Replace components
                    backedUpActor.GetComponentIndices().Except(new[] { ActorComponentsLookup.Backup }).ToArray()); //Copy everything except the backup-component
            }

            /*
            * ====================== Revert game-entities ======================      
            */

            var currentEntities = _gameContext.GetEntities(GameMatcher.LocalId);
            var backupEntities = _gameContext.GetEntities(GameMatcher.Backup).Where(e => e.backup.tick == resultTick).ToList();
            var backupEntityIds = backupEntities.Select(entity => entity.backup.localEntityId);

            //Entities that were created in the prediction have to be destroyed  
            var invalidEntities = currentEntities.Where(entity => !backupEntityIds.Contains(entity.localId.value)).ToList();
            foreach (var invalidEntity in invalidEntities)
            {
                //Here we have the actual entities, we can safely refer to them via the internal id
                _view.DeleteView(invalidEntity.localId.value);
                invalidEntity.Destroy();
            }

            foreach (var invalidBackupEntity in _gameContext.GetEntities(GameMatcher.Backup).Where(e => e.backup.tick > resultTick))
            {
                Services.Get<ISnapshotIndexService>().RemoveIndex(invalidBackupEntity.backup.tick);
                invalidBackupEntity.Destroy();
            }

            //Copy old state to the entity                                      
            foreach (var backupEntity in backupEntities)
            {
                var target = _gameContext.GetEntityWithLocalId(backupEntity.backup.localEntityId);
                var additionalComponentIndices = target.GetComponentIndices().Except(
                        backupEntity
                            .GetComponentIndices()
                            .Except(new[] { GameComponentsLookup.Backup })
                            .Concat(new[] { GameComponentsLookup.Id, GameComponentsLookup.ActorId, GameComponentsLookup.LocalId }));
                foreach (var index in additionalComponentIndices)
                {
                    target.RemoveComponent(index);
                }

                backupEntity.CopyTo(target, true, backupEntity.GetComponentIndices().Except(new[] { GameComponentsLookup.Backup }).ToArray());
            }

            //TODO: restore locally destroyed entities   

            Contexts.gameState.ReplaceTick(resultTick);
        }

        private void Predict()
        {
            if (!Contexts.gameState.isPredicting)
            {
                Contexts.gameState.isPredicting = true;
            }

            _logger.Trace("Predict " + CurrentTick);

            Execute();
            Cleanup();
        }

        private void Simulate()
        {
            if (Contexts.gameState.isPredicting)
            {
                Contexts.gameState.isPredicting = false;
            }

            _logger.Trace("Simulate " + CurrentTick);

            Execute();
            Cleanup();

            Services.Get<IDebugService>().Register(Contexts.gameState.tick.value, Contexts.gameState.hashCode.value);
        }

        private void Tick()
        {
            lock (_commandCache)
            {
                if (_commandCache.Count > 0)
                {
                    AddInput(CurrentTick + LagCompensation, LocalActorId, _commandCache);
                    if (SendCommandsToBuffer)
                    {
                        _remoteCommandBuffer.Insert(CurrentTick + LagCompensation, LocalActorId, _commandCache.ToArray());
                    }

                    _commandCache.Clear();
                }
            }

            Predict();
        }

        private void SyncCommandBuffer()
        {
            var commands = _remoteCommandBuffer.GetChanges();

            if (commands.Count > 0)
            {
                //We guess everything was predicted correctly (except the last received frame)
                var firstMispredictedFrame = commands.Keys.Min();
                var lastInputFrame = commands.Keys.Max();


                _logger.Trace(">>>Input from " + firstMispredictedFrame + " to " + lastInputFrame);

                foreach (var tick in commands.Keys)
                {
                    foreach (var actorId in commands[tick].Keys)
                    {
                        AddInput(tick, actorId, commands[tick][actorId]);
                    }
                }

                //Only rollback if the mispredicted frame was in the past (the frame can be in the future due to high lag compensation)
                if (firstMispredictedFrame <= CurrentTick)
                {
                    var targetTick = CurrentTick;

                    RevertToTick(firstMispredictedFrame);

                    //Restore last local state       
                    while (CurrentTick <= lastInputFrame)
                    {
                        Simulate();
                    }

                    while (CurrentTick < targetTick)
                    {
                        Predict();
                    }
                }
            }
        }

        /// <summary>
        /// Experimental
        /// </summary>
        private void StartAsThread()
        {
            new Thread(Loop) { IsBackground = true }.Start();
        }

        private void Loop()
        {
            var timer = new Timer();

            Running = true;

            timer.Start();

            while (Running)
            {
                Update(timer.Tick());

                Thread.Sleep(1);
            }
        }
    }
}