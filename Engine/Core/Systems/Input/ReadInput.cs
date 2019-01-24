using Entitas;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Systems.Input
{
    public class ReadInput : IExecuteSystem, ICleanupSystem
    {                                              
        private readonly InputContext _inputContext;

        private readonly IFrameDataSource _dataSource;

        public ReadInput(Contexts contexts, IFrameDataSource dataSource)
        {                                  
            _inputContext = contexts.input;
            _dataSource = dataSource;
        }     

        public void Execute()
        {
            _inputContext.SetFrame(_dataSource.GetNext());
        }

        public void Cleanup()
        {
            _inputContext.DestroyAllEntities();
        }
    }
}     
