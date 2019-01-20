using Entitas;

namespace ECS.Systems.Input
{
    public class EmitInputSystem : IExecuteSystem, ICleanupSystem
    {
        private readonly GameContext _gameContext;
        private readonly InputContext _inputContext;

        private readonly IParseInputService _parseInputService;

        public EmitInputSystem(Contexts contexts, IParseInputService parseInputService)
        {
            _gameContext = contexts.game;
            _inputContext = contexts.input;
            _parseInputService = parseInputService;
        }     

        public void Execute()
        {
            if (_inputContext.frame.SerializedInputs == null)
                return;

            foreach (var input in _inputContext.frame.SerializedInputs)
            {
                _parseInputService.Parse(_gameContext, input);
            }
        }

        public void Cleanup()
        {
            _inputContext.DestroyAllEntities();
        }
    }
}     
