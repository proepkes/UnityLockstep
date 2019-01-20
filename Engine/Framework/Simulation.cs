using System.Collections.Generic;  
using BEPUphysics;
using ECS;
using ECS.Data;
using ECS.Features;
using FixMath.NET;     

namespace Lockstep.Framework
{
    public class Simulation
    {
        private readonly Contexts _contexts;
        private readonly LockstepSystems _systems;   

        public Space Space { get; }


        public int FrameDelay { get; set; }

        public uint FrameCounter { get; private set; }

        public long HashCode => _contexts.gameState.gameHashCode.value;


        private uint _lastFramePointer;

        private Fix64Random _random;                  

        private readonly Dictionary<uint, Frame> _frames = new Dictionary<uint, Frame>();
             
        public bool CanSimulate
        {
            get
            {
                lock (_frames)
                {
                    return _lastFramePointer - FrameCounter - FrameDelay > 0;
                }
            }
        }

        public Simulation(Contexts contexts, ServiceContainer serviceContainer)
        {
            _contexts = contexts;
            Space = new Space();

            _systems = new LockstepSystems(contexts, serviceContainer);
        }
          

        public Simulation Init(int seed)
        {  
            _random = new Fix64Random(seed);    
            
            _systems.Initialize();                      
            return this;
        }


        public Simulation AddFrame(Frame frame)
        {
            lock (_frames)
            {      
                _frames[_lastFramePointer++] = frame;
            }

            return this;
        }
 


        public Fix64 NextRandom()
        {
            return _random.Next();
        }


        public Simulation Simulate()
        {
            Frame currentFrame;
            lock (_frames)
            {
                currentFrame = _frames[FrameCounter++];
            }       

            _systems.Simulate(currentFrame);    

            //if (!CanSimulate)
            //{
            //    return;
            //}

            //Frame currentFrame;
            //lock (_frames)
            //{
            //    currentFrame = _frames[FrameCounter++];
            //}

            //lock (_pendingEntities)
            //{                                         
            //    foreach (var entity in _pendingEntities)
            //    {
            //        entity.ID = _entityCounter;
            //        _entities[_entityCounter] = entity;
            //        if (entity is ILockstepAgent agent)
            //        {
            //            Space.Add(agent.Body);
            //        }

            //        _entityCounter++;
            //    }
            //    _pendingEntities.Clear();
            //} 

            //foreach (var serializedInput in currentFrame.SerializedInputs)
            //{
            //    _commandHandler.Handle(serializedInput);   
            //}  

            //foreach (var entity in _entities.Values)
            //{
            //    entity.Simulate();                        
            //}
                      
            //Space.Update();

            return this;
        }      
    }
}