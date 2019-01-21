using System.Diagnostics;

namespace Server
{
    public class Timer
    {
        private long _lastTick;

        public long DeltaTime { get; private set; }
        public uint TickCount { get; private set; }

        private readonly Stopwatch _sw = new Stopwatch();
        public void Start()
        {
            _sw.Start();
        }

        public void Tick()
        {
            DeltaTime = _sw.ElapsedMilliseconds - _lastTick;
            _lastTick = _sw.ElapsedMilliseconds;
            TickCount++;                   
        }
    }
}