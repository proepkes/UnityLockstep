using System;
using System.Collections.Generic;
using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;
using Lockstep.Network.Messages;

namespace Lockstep.Client
{
    public class Simulation
    {                              
        public event Action<long> Ticked;     

        /// <summary>
        /// Amount of ticks until a command gets executed
        /// </summary>
        public int LagCompensation { get; set; } = 10;

        public byte LocalPlayerId { get; private set; }   

        public bool Running { get; set; }   

        public long CurrentTick { get; private set; }

        public readonly ICommandBuffer CommandBuffer;

        public float _tickDt;
        public float _accumulatedTime;
        private readonly ISystems _systems;
        private readonly List<ICommand> _temporaryCommandBuffer = new List<ICommand>(20);


        public Simulation(ISystems systems, ICommandBuffer commandBuffer)
        {
            _systems = systems;                       

            CommandBuffer = commandBuffer;    
        }

        public void Start(Init init)
        {             
            _tickDt = 1000f / init.TargetFPS;
            LocalPlayerId = init.PlayerID;

            _systems.Initialize();

            Running = true;
        }   

        public void Execute(ICommand command)
        {
            lock (_temporaryCommandBuffer)
            {
                _temporaryCommandBuffer.Add(command);
            }            
        }

        public void Update(float deltaTime)
        {                             
            if (!Running)                        
            {
                return;
            }
                   
            SyncCommandBuffer();

            _accumulatedTime += deltaTime; 

            while (_accumulatedTime >= _tickDt)
            {
                Tick();

                _accumulatedTime -= _tickDt;
            }                 
        }

        private void Tick()
        {           
            ICommand[] frameCommands;
            lock (_temporaryCommandBuffer)
            {
                frameCommands = _temporaryCommandBuffer.ToArray();
                _temporaryCommandBuffer.Clear();
            }


            //Commit our input to the buffer, make sure we don't overwrite any other input by locking it
            //Possible scenario without locking:
            // 1. buffer index matches our current tick, no rollback required
            // 2. buffer receives old input (index gets set to the past)
            // 3. because of (1) we don't roll back and insert our input
            // 4. buffer index is set to our currenttick, so the input from (2) would never be executed
            CommandBuffer.Lock();
            SyncCommandBuffer();

            if (frameCommands.Length > 0)
            {
                CommandBuffer.Insert(LocalPlayerId, CurrentTick + LagCompensation, frameCommands);
            }

            _systems.SetInput(CommandBuffer.GetNext());
            CommandBuffer.Release();

            _systems.Tick();

            Ticked?.Invoke(CurrentTick);

            CurrentTick++;
        }

        private void SyncCommandBuffer()
        {
            while (CommandBuffer.NextFrameIndex < CurrentTick)
            {
                //The buffer contains inputs from the past, rollback and execute them
                
                //TODO: real rollback :).. this method works if there is only one player who sends commands because commands from other players dont have to be reverted
                _systems.SetInput(CommandBuffer.GetNext());  
                _systems.Tick();

            }
        }
    }
}