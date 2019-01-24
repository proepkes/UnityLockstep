using ECS;
using ECS.Data;                                      

namespace Lockstep.Framework
{
    public class Simulation
    {
        private readonly LockstepSystems _systems;
                                                  
        private readonly IFrameDataSource _dataSource;                   

        public long HashCode => _systems.HashCode;     

        public Simulation(Contexts contexts, ServiceContainer serviceContainer)
        {
            _dataSource = serviceContainer.Get<IFrameDataSource>();
            if (_dataSource == null)
            {
                _dataSource = new FrameDataSource();
                serviceContainer.Register(_dataSource);
            }             

            _systems = new LockstepSystems(contexts, serviceContainer);
        }
              

        public Simulation Init()
        {                                   
            _systems.Initialize();
            return this;
        }

        public Simulation AddFrame(Frame frame)
        {
            _dataSource.Insert(frame);

            return this;
        }
        public Simulation Simulate()
        {
            _systems.Simulate();
            return this;
        }        
    }
}