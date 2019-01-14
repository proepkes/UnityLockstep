using System;
using BEPUphysics.Entities;

namespace Lockstep.Framework.Pathfinding
{
    public class LSInfluencer
    {
        #region Static Helpers

        static LockstepAgent tempAgent;
        static GridNode tempNode;

        #endregion

        #region Collection Helper

        [NonSerialized]
        public int bucketIndex = -1;

        #endregion

        #region ScanNode Helper

        public int NodeTicket;

        #endregion

        public GridNode LocatedNode { get; private set; }

        public Entity Body { get; private set; }

        public LockstepAgent Agent { get; private set; }

        public void Setup(LockstepAgent agent)
        {
            Agent = agent;
            Body = agent.Body;
        }

        public void Initialize()
        {
            LocatedNode = GridManager.GetNode(Body.Position.X, Body.Position.Y);

            LocatedNode.Add(this);
        }

        public void Simulate()
        {      
            //if (Body.PositionChangedBuffer)
            //{
            //    tempNode = GridManager.GetNode(Body.Position.X, Body.Position.Y);

            //    if (tempNode.IsNull())
            //        return;
				
            //    if (System.Object.ReferenceEquals(tempNode, LocatedNode) == false)
            //    {
            //        if (LocatedNode != null)
            //            LocatedNode.Remove(this);
            //        tempNode.Add(this);
            //        LocatedNode = tempNode;
            //    }
            //}
        }

        public void Deactivate()
        {
            if (LocatedNode != null)
            {
                LocatedNode.Remove(this);
                LocatedNode = null;
            }
        }

    }


}