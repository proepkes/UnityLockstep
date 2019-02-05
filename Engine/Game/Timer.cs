using System.Diagnostics;

namespace Lockstep.Game
{
    public class Timer
    {
        private long _lastTick;
                                                      
        public uint TickCount { get; private set; }

        private readonly Stopwatch _sw = new Stopwatch();
        public void Start()
        {
            _sw.Start();
        }

        public long Tick()
        {
            var elapsed = _sw.ElapsedMilliseconds;
            var dt = elapsed - _lastTick;
            _lastTick = elapsed;
            TickCount++;

            return dt;
        }
    }
}