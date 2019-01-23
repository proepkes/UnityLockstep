using System.Collections.Generic; 
using ECS;
using ECS.Data;
using FixMath.NET;     

namespace Lockstep.Framework
{
    public class Simulation : IInputService
    {                                          
        private readonly LockstepSystems _systems;   

        public int FrameDelay { get; set; }

        public uint FrameCounter { get; private set; }

        public long HashCode => _systems.HashCode;


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
            serviceContainer.Register<IInputService>(this);

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
            _systems.Simulate(); 
            return this;
        }     

        public Frame GetNextFrame()
        {
            Frame nextFrame;
            lock (_frames)
            {
                nextFrame = _frames[FrameCounter++];
            }
            return nextFrame;
        }
    }
}