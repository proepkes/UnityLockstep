using Entitas;

namespace ECS.Systems.Input
{
    public class ReadInput : IExecuteSystem, ICleanupSystem
    {                                              
        private readonly InputContext _inputContext;

        private readonly IDataSource _dataSource;

        public ReadInput(Contexts contexts, IDataSource dataSource)
        {                                  
            _inputContext = contexts.input;
            _dataSource = dataSource;
        }     

        public void Execute()
        {
            _inputContext.SetFrame(_dataSource.GetNextFrame());
        }

        public void Cleanup()
        {
            _inputContext.DestroyAllEntities();
        }
    }
}     
