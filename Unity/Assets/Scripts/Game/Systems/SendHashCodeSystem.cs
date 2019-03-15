using System;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Game.Systems
{
    [DisableAutoCreation, DependencySystem]  
    public class SendHashCodeSystem : ComponentSystem
    {
        private readonly INetwork network;
        private ulong tick = 0L;

        public SendHashCodeSystem(INetwork network)
        {
            this.network = network;
        }

        protected override void OnUpdate()
        {
           var result = 0;
           Entities.ForEach((ref Translation t, ref Rotation r) =>
           {
               result += t.Value.GetHashCode();
               //result += r.Value.GetHashCode();

           });

           var data1 = BitConverter.GetBytes(tick++);
           var data2 = BitConverter.GetBytes(result);
           var data = data1.Concat(data2).ToArray();

           network.Send(data);
        }
    }
}