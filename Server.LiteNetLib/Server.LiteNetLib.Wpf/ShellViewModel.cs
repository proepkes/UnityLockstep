using System.Threading;
using System.Threading.Tasks;
using Lockstep.Network.Server;
using Stylet;

namespace Server.LiteNetLib.Wpf
{

    class ShellViewModel : Screen
    {
        public TickMonitorViewModel TickMonitorViewModel { get; }
        public int RoomSize { get; set; } = 1;

        public bool Listening { get; set; }
        
        private Room _room;
        private readonly LiteNetLibServer _server = new LiteNetLibServer();
        private Task _serverTask;

        public ShellViewModel(TickMonitorViewModel tickMonitorViewModel)
        {
            TickMonitorViewModel = tickMonitorViewModel;

            DisplayName = "UnityLockstep - Server";

        }

        public bool CanStartServer => !Listening;
        public void StartServer()
        {
            Listening = true;

            _room = new Room(_server, RoomSize);
            _room.Starting += OnRoomStarting;

            _room.InputReceived += OnRoomInputReceived;

            _room.Open(9050);


            //A Task to poll for server-events. Will also increase the server's tick-counter as long as the simulation is running
            _serverTask = Task.Factory.StartNew(() =>
            {
                while (Listening)
                {
                    _server.PollEvents();

                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public bool CanStopServer => Listening;
        public void StopServer()
        {
            Listening = false;

            _serverTask.Wait();

            _room.Starting -= OnRoomStarting;
            _room.InputReceived -= OnRoomInputReceived;
            _room.Close();
            _room = null;
        }

        public bool CanIncrementRoomSize => !Listening;
        public void IncrementRoomSize()
        {
            RoomSize += 1;
        }

        public bool CanDecrementRoomSize => !Listening && RoomSize > 1;
        public void DecrementRoomSize()
        {
            RoomSize -= 1;
        }

        private void OnRoomInputReceived(object sender, InputReceivedEventArgs args)
        {
            Execute.OnUIThread(() => { TickMonitorViewModel.AddTick(args.ActorId, args.Tick); });
        }

        private void OnRoomStarting(object sender, StartedEventArgs args)
        {                         
            Execute.OnUIThread(() => { TickMonitorViewModel.StartNew(args.SimulationSpeed, args.ActorIds); });  
        }
    }
}
