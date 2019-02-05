using Entitas;

namespace Lockstep.Game.Systems.Input
{
    public class CleanupInput : ICleanupSystem
    {
        private readonly InputContext _inputContext;

        public CleanupInput(Contexts contexts)
        {
            _inputContext = contexts.input;
        }

        public void Cleanup()
        {   
            _inputContext.DestroyAllEntities();
        }
    }
}
