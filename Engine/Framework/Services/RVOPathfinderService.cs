using System.Collections.Generic;
using System.Linq;
using BEPUutilities;
using FixMath.NET;
using RVO;

namespace Lockstep.Framework.DefaultServices
{
    public class RVOPathfinderService : IPathfindingService

    {
        internal KdTree kdTree_;
        internal IList<Agent> agents_;
                                                    
        private Agent defaultAgent_;

        public RVOPathfinderService()
        {
            kdTree_ = new KdTree();
            agents_ = new List<Agent>();
            setAgentDefaults(15, 10, 5, 5, 2, 2, Vector2.Zero);
        }

        public void AddAgent(GameEntity entity, Vector2 position)
        {
            agents_.Add(new Agent(entity.id.value)
            {                   
                maxNeighbors_ = defaultAgent_.maxNeighbors_,
                maxSpeed_ = defaultAgent_.maxSpeed_,
                neighborDist_ = defaultAgent_.neighborDist_,
                position_ = position,
                radius_ = defaultAgent_.radius_,
                timeHorizon_ = defaultAgent_.timeHorizon_,
                timeHorizonObst_ = defaultAgent_.timeHorizonObst_,
                velocity_ = defaultAgent_.velocity_
            });                                                         
        }

        public void UpdateAgents(GameEntity[] entities)
        {
            //Calc. preferred velocity
            foreach (var entity in entities)
            {
                Vector2 goalVector = entity.destination.value - entity.position.value;

                if (goalVector.LengthSquared() > Fix64.One)
                {
                    goalVector = Vector2.Normalize(goalVector);
                }
                                                        
                agents_.First(a => a.Id == entity.id.value).prefVelocity_ = goalVector;
            }

            kdTree_.buildAgentTree(agents_);

            foreach (var agent in agents_)
            {
                agent.computeNeighbors(kdTree_);
                agent.computeNewVelocity();                         
            }

            foreach (var agent in agents_)
            {                               
                agent.update();
            }
        }

        public Vector2 GetAgentPosition(int agentId)
        {
            return agents_.First(agent => agent.Id == agentId).position_;
        }

        public void setAgentDefaults(Fix64 neighborDist, int maxNeighbors, Fix64 timeHorizon, Fix64 timeHorizonObst, Fix64 radius, Fix64 maxSpeed, Vector2 velocity)
        {
            if (defaultAgent_ == null)
            {
                defaultAgent_ = new Agent(-1);
            }

            defaultAgent_.maxNeighbors_ = maxNeighbors;
            defaultAgent_.maxSpeed_ = maxSpeed;
            defaultAgent_.neighborDist_ = neighborDist;
            defaultAgent_.radius_ = radius;
            defaultAgent_.timeHorizon_ = timeHorizon;
            defaultAgent_.timeHorizonObst_ = timeHorizonObst;
            defaultAgent_.velocity_ = velocity;
        }      
    }
}
