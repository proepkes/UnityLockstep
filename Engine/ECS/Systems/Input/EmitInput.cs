using Entitas;

namespace ECS.Systems.Input
{
    public class EmitInput : IExecuteSystem, ICleanupSystem
    {                                              
        private readonly InputContext _inputContext;    

        public EmitInput(Contexts contexts)
        {                                  
            _inputContext = contexts.input;    
        }     

        public void Execute()
        {
            if (_inputContext.frame.value.Commands == null)
                return;

            foreach (var command in _inputContext.frame.value.Commands)
            {
                command.Execute(_inputContext);       
            }
        }

        public void Cleanup()
        {
            _inputContext.DestroyAllEntities();
        }
    }
}     
