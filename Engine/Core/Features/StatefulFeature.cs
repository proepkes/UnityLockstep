using Lockstep.Core.Interfaces;

namespace Lockstep.Core.Features
{
    public abstract class StatefulFeature : Feature, IStateSystem
    {     
        public void RevertFromTick(uint tick)
        {
            foreach (var system in _executeSystems)
            {
                if (system is IStateSystem stateSystem)
                {
                    stateSystem.RevertFromTick(tick);
                }
            }
        }
    }
}
