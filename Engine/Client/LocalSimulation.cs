using Lockstep.Core.Data;
using Lockstep.Core.Interfaces;

namespace Lockstep.Client
{
    /// <summary>
    /// This simulation immediately executes a command by adding a new frame and ticking the systems. You can manually step the simulation by accessing the Systems property
    /// </summary>
    public class LocalSimulation
    {
        public ISystems Systems { get; }


        public LocalSimulation(ISystems systems)
        {                      
            Systems = systems;
            Systems.Initialize();
        }

        public void Execute(ICommand command)
        {
            Systems.DataSource.Insert(new Frame() {  Commands = new []{ command }});
            Systems.Tick();
        }
    }
}
