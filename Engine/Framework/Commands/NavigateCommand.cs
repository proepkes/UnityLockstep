using BEPUutilities;   
using Lockstep.Framework.Networking;
using Lockstep.Framework.Services;

namespace Lockstep.Framework.Commands
{
    public class NavigateCommand : CommandBase
    {
        public int[] EntityIds;

        public Vector2 Destination;  

        public NavigateCommand() : base(CommandTag.Navigate)
        {
        }      

        protected override void OnSerialize(INetworkWriter writer)
        {
            writer.PutArray(EntityIds);
            writer.Put((long)Destination.X);
            writer.Put((long)Destination.Y);
        }

        protected override void OnDeserialize(INetworkReader reader)
        {
            EntityIds = reader.GetIntArray();
            Destination.X = reader.GetLong();
            Destination.Y = reader.GetLong();
        }

        public override void Execute(InputContext context)
        {
            var e = context.CreateEntity();
            e.AddNavigationInputData(EntityIds, Destination);                              
        }
    }
}
