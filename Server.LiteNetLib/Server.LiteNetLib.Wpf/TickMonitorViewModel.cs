using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Stylet;
using Timer = Lockstep.Common.Timer;

namespace Server.LiteNetLib.Wpf
{
    public class TickModel
    {
        public DateTime DateTime { get; }
        public uint Value { get; }

        public TickModel(DateTime time, uint value)
        {
            DateTime = time;
            Value = value;
        }
    }

    class TickMonitorViewModel : Screen
    {
        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }

        public double XAxisStep { get; set; }
        public double XAxisUnit { get; set; }
        public double XAxisMax { get; set; }
        public double XAxisMin { get; set; }
        public double YAxisMax { get; set; }
        public double YAxisMin { get; set; }
        
        private float _tickDt;
        private uint _currentTick = 1;
        
        private readonly Timer _tickTimer = new Timer();
        
        private DateTime _startTime = DateTime.Now;

        private readonly Dictionary<int, IChartValues> _tickDataPerClient = new Dictionary<int, IChartValues>();

        public TickMonitorViewModel()
        {
            DateTimeFormatter = value =>
            {
                var diff = (long) value - _startTime.Ticks;
                return  diff < 0 ? "" : new DateTime(diff).ToString("mm:ss");
            };

            //Force the distance between each separator in the X axis
            XAxisStep = TimeSpan.FromSeconds(1).Ticks;

            //Let the axis know that we are plotting seconds (this is not always necessary, but it can prevent wrong labeling)
            XAxisUnit = TimeSpan.TicksPerSecond;
            
            SeriesCollection = new SeriesCollection(
                Mappers.Xy<TickModel>()
                    .X(model => model.DateTime.Ticks)
                    .Y(model => model.Value));

            SetAxisLimits(DateTime.Now);

            Task.Factory.StartNew(() =>
            {
                float accumulatedTime = 0f;

                while (true)
                {
                    SetAxisLimits(DateTime.Now);

                    accumulatedTime += _tickTimer.Tick();

                    if (accumulatedTime >= _tickDt)
                    {
                        _currentTick++;

                        accumulatedTime -= _tickDt;
                    }
                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void StartNew(int optimumFps, byte[] actorIds)
        {
            _tickDataPerClient.Clear();

            SeriesCollection.Clear();

            foreach (var actorId in actorIds)
            {
                var lineSeries = new LineSeries
                {
                    Values = new ChartValues<TickModel>(),
                    Title = "Actor " + actorId,
                };

                _tickDataPerClient.Add(actorId, lineSeries.Values);
                SeriesCollection.Add(lineSeries);
            }

            _currentTick = 1;
            _startTime = DateTime.Now;
            _tickDt = 1000f / optimumFps;
            _tickTimer.Start();
        }
        
        public void AddTick(byte actorId, uint value)
        {
            _tickDataPerClient[actorId].Add(new TickModel(DateTime.Now, _currentTick - value));


            if (_tickDataPerClient[actorId].Count > 250)
                _tickDataPerClient[actorId].RemoveAt(0);

        }

        private void SetAxisLimits(DateTime now)
        {
            XAxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // force the axis to be 1 second ahead
            XAxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
        }
    }
}
