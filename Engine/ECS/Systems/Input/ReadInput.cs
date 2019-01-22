using Entitas;

namespace ECS.Systems.Input
{
    public class ReadInput : IExecuteSystem, ICleanupSystem
    {                                              
        private readonly InputContext _inputContext;

        private readonly IInputService _inputService;

        public ReadInput(Contexts contexts, IInputService inputService)
        {                                  
            _inputContext = contexts.input;
            _inputService = inputService;
        }     

        public void Execute()
        {
            _inputContext.SetFrame(_inputService.ReadNextFrame());
        }

        public void Cleanup()
        {
            _inputContext.DestroyAllEntities();
        }
    }
}     
