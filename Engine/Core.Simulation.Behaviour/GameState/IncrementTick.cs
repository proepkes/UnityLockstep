using Entitas;

namespace Lockstep.Core.Systems.GameState
{
    public class IncrementTick : IInitializeSystem, IExecuteSystem
    {
        private readonly GameStateContext _gameStateContext;

        public IncrementTick(Contexts contexts)
        {
            _gameStateContext = contexts.gameState;
        }
        public void Initialize()
        {
            _gameStateContext.SetTick(0);
        }

        public void Execute()
        {                                              
            _gameStateContext.tickEntity.ReplaceTick(_gameStateContext.tick.value + 1);
        }   
    }
}
