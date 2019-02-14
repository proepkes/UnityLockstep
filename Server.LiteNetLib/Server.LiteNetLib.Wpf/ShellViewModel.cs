using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Lockstep.Network.Server;
using Stylet;
using Timer = Lockstep.Common.Timer;

namespace Server.LiteNetLib.Wpf
{
    public class TickModel
    {
        public DateTime DateTime { get; set; }
        public uint Value { get; set; }

        public TickModel(DateTime time, uint value)
        {
            DateTime = time;
            Value = value;
        }
    }

    class ShellViewModel : Screen
    {

        public int RoomSize { get; set; } = 1;

        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public bool SimulationRunning { get; set; }

        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }
        public double XAxisMax { get; set; }
        public double XAxisMin { get; set; }
        public double YAxisMax { get; set; }
        public double YAxisMin { get; set; }

        public TimeSpan AnimationSpeed => TimeSpan.FromMilliseconds(50);

        private uint _currentTick = 0;

        private float _tickDt;
        private readonly Timer _tickTimer = new Timer();
        private readonly LiteNetLibServer _server = new LiteNetLibServer();
        private readonly Dictionary<int, IChartValues> _tickDataPerClient = new Dictionary<int, IChartValues>();

        public ShellViewModel()
        {
            DisplayName = "UnityLockstep - Server";

            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            //Force the distance between each separator in the X axis
            AxisStep = TimeSpan.FromSeconds(1).Ticks;

            //Let the axis know that we are plotting  seconds (this is not always necessary, but it can prevent wrong labeling)
            AxisUnit = TimeSpan.TicksPerSecond;

            SetAxisLimits(DateTime.Now);

            var dayConfig = Mappers.Xy<TickModel>()
                .X(model => model.DateTime.Ticks)
                .Y(model => model.Value);

            SeriesCollection = new SeriesCollection(dayConfig);



            Task.Factory.StartNew(() =>
            {                                        
                while (true)
                {
                    SetAxisLimits(DateTime.Now);
                    Thread.Sleep(20);
                }
            }, TaskCreationOptions.LongRunning);

        }

        public bool CanStartServer { get; set; } = true;
        public void StartServer()
        {
            CanStartServer = false;

            var room = new Room(_server, RoomSize);
            room.Starting += (sender, args) =>
            {
                _currentTick = 0;
                _tickDt = 1000f / args.SimulationSpeed;

                SimulationRunning = true;

                _tickTimer.Start();

                Execute.OnUIThread(() =>
                {
                    foreach (var actorId in args.ActorIds)
                    {
                        var lineSeries = new LineSeries
                        {
                            Values = new ChartValues<TickModel>(),
                            Title = "Actor " + actorId,
                        };
                        
                        _tickDataPerClient.Add(actorId, lineSeries.Values);
                        SeriesCollection.Add(lineSeries);
                    }
                });
            };

            room.InputReceived += (sender, args) =>
            {
                Execute.OnUIThread(() =>
                {
                    _tickDataPerClient[args.ActorId].Add(new TickModel(DateTime.Now, _currentTick - args.Tick));


                    if (_tickDataPerClient[args.ActorId].Count > 250)
                        _tickDataPerClient[args.ActorId].RemoveAt(0);
                });
            };

            room.Open(9050);


            //A Task to poll for server-events. Will also increase the server's tick-counter as long as the simulation is running
            Task.Factory.StartNew(() =>
            {
                float accumulatedTime = 0f;
                
                while (true)
                {
                    _server.PollEvents();

                    if (SimulationRunning)
                    {
                        accumulatedTime += _tickTimer.Tick();

                        if (accumulatedTime >= _tickDt)
                        {
                            _currentTick++;

                            accumulatedTime -= _tickDt;
                        }
                    }

                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void SetAxisLimits(DateTime now)
        {
            XAxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // force the axis to be 1 second ahead
            XAxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
        }
    }
}
