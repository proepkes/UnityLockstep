using System.Collections.Generic;
using Entitas;
using FixMath.NET;
using Lockstep.Core.State;
using Lockstep.Game.Features.Navigation.RVO.Algorithm;

namespace Lockstep.Core.Logic.Systems.Game
{
    public class BuildKdTree : IInitializeSystem, IExecuteSystem
    {
        private const int MAX_LEAF_SIZE = 10;

        private readonly GameContext _gameContext;
        private GameEntity[] _entities;

        public BuildKdTree(Contexts contexts)
        {
            _gameContext = contexts.game;
        }

        public void Initialize()
        {
            _gameContext.SetKdTree(true, new AgentTreeNode[0]);
        }

        public void Execute()
        {
            _entities = _gameContext.GetEntities(GameMatcher.LocalId);

            var kdTree = _gameContext.kdTree;

            kdTree.AgentTree = new AgentTreeNode[2 * _entities.Length];

            for (int i = 0; i < kdTree.AgentTree.Length; ++i)
            {
                kdTree.AgentTree[i] = new AgentTreeNode();
            }

            if (_entities.Length > 0)
            {
                buildAgentTreeRecursive(ref kdTree.AgentTree, _entities, 0, _entities.Length, 0);
            }

            foreach (var e in _entities)
            {                   
                var rangeSq = RVOMath.sqr(e.rvoAgentSettings.neighborDist);
                queryAgentTreeRecursive(ref _gameContext.kdTree.AgentTree, e, ref rangeSq, 0);
            }
        }      

        private void buildAgentTreeRecursive(ref AgentTreeNode[] agentTree_, GameEntity[] agents_, int begin, int end, int node)
        {
            agentTree_[node].begin_ = begin;
            agentTree_[node].end_ = end;
            agentTree_[node].minX_ = agentTree_[node].maxX_ = agents_[begin].position.value.X;
            agentTree_[node].minY_ = agentTree_[node].maxY_ = agents_[begin].position.value.Y;

            for (int i = begin + 1; i < end; ++i)
            {
                agentTree_[node].maxX_ = RVOMath.Max(agentTree_[node].maxX_, agents_[i].position.value.X);
                agentTree_[node].minX_ = RVOMath.Min(agentTree_[node].minX_, agents_[i].position.value.X);
                agentTree_[node].maxY_ = RVOMath.Max(agentTree_[node].maxY_, agents_[i].position.value.Y);
                agentTree_[node].minY_ = RVOMath.Min(agentTree_[node].minY_, agents_[i].position.value.Y);
            }

            if (end - begin > MAX_LEAF_SIZE)
            {
                /* No leaf node. */
                bool isVertical = agentTree_[node].maxX_ - agentTree_[node].minX_ > agentTree_[node].maxY_ - agentTree_[node].minY_;
                Fix64 splitValue = 0.5m * (isVertical ? agentTree_[node].maxX_ + agentTree_[node].minX_ : agentTree_[node].maxY_ + agentTree_[node].minY_);

                int left = begin;
                int right = end;

                while (left < right)
                {
                    while (left < right && (isVertical ? agents_[left].position.value.X : agents_[left].position.value.Y) < splitValue)
                    {
                        ++left;
                    }

                    while (right > left && (isVertical ? agents_[right - 1].position.value.X : agents_[right - 1].position.value.Y) >= splitValue)
                    {
                        --right;
                    }

                    if (left < right)
                    {
                        GameEntity tempAgent = agents_[left];
                        agents_[left] = agents_[right - 1];
                        agents_[right - 1] = tempAgent;
                        ++left;
                        --right;
                    }
                }

                int leftSize = left - begin;

                if (leftSize == 0)
                {
                    ++leftSize;
                    ++left;
                    ++right;
                }

                agentTree_[node].left_ = node + 1;
                agentTree_[node].right_ = node + 2 * leftSize;

                buildAgentTreeRecursive(ref agentTree_, agents_, begin, left, agentTree_[node].left_);
                buildAgentTreeRecursive(ref agentTree_, agents_, left, end, agentTree_[node].right_);
            }
        }

        private void queryAgentTreeRecursive(ref AgentTreeNode[] agentTree_, GameEntity agent, ref Fix64 rangeSq, int node)
        {
            if (agentTree_[node].end_ - agentTree_[node].begin_ <= MAX_LEAF_SIZE)
            {
                for (int i = agentTree_[node].begin_; i < agentTree_[node].end_; ++i)
                {                                                         
                    if (agent != _entities[i])
                    {
                        Fix64 distSq = (agent.position.value - _entities[i].position.value).LengthSquared();

                        if (distSq < rangeSq)
                        {
                            if (agent.rvoAgentSettings.agentNeighbors.Count < agent.rvoAgentSettings.maxNeighbors)
                            {
                                agent.rvoAgentSettings.agentNeighbors.Add(new KeyValuePair<Fix64, GameEntity>(distSq, _entities[i]));
                            }

                            int j = agent.rvoAgentSettings.agentNeighbors.Count - 1;

                            while (j != 0 && distSq < agent.rvoAgentSettings.agentNeighbors[j - 1].Key)
                            {
                                agent.rvoAgentSettings.agentNeighbors[j] = agent.rvoAgentSettings.agentNeighbors[j - 1];
                                --j;
                            }

                            agent.rvoAgentSettings.agentNeighbors[j] = new KeyValuePair<Fix64, GameEntity>(distSq, _entities[i]);

                            if (agent.rvoAgentSettings.agentNeighbors.Count == agent.rvoAgentSettings.maxNeighbors)
                            {
                                rangeSq = agent.rvoAgentSettings.agentNeighbors[agent.rvoAgentSettings.agentNeighbors.Count - 1].Key;
                            }    
                        }
                    }
                }
            }
            else
            {
                Fix64 distSqLeft = RVOMath.sqr(RVOMath.Max(Fix64.Zero, agentTree_[agentTree_[node].left_].minX_ - agent.position.value.X)) + RVOMath.sqr(RVOMath.Max(Fix64.Zero, agent.position.value.X - agentTree_[agentTree_[node].left_].maxX_)) + RVOMath.sqr(RVOMath.Max(Fix64.Zero, agentTree_[agentTree_[node].left_].minY_ - agent.position.value.Y)) + RVOMath.sqr(RVOMath.Max(Fix64.Zero, agent.position.value.Y - agentTree_[agentTree_[node].left_].maxY_));
                Fix64 distSqRight = RVOMath.sqr(RVOMath.Max(Fix64.Zero, agentTree_[agentTree_[node].right_].minX_ - agent.position.value.X)) + RVOMath.sqr(RVOMath.Max(Fix64.Zero, agent.position.value.X - agentTree_[agentTree_[node].right_].maxX_)) + RVOMath.sqr(RVOMath.Max(Fix64.Zero, agentTree_[agentTree_[node].right_].minY_ - agent.position.value.Y)) + RVOMath.sqr(RVOMath.Max(Fix64.Zero, agent.position.value.Y - agentTree_[agentTree_[node].right_].maxY_));

                if (distSqLeft < distSqRight)
                {
                    if (distSqLeft < rangeSq)
                    {
                        queryAgentTreeRecursive(ref agentTree_, agent, ref rangeSq, agentTree_[node].left_);

                        if (distSqRight < rangeSq)
                        {
                            queryAgentTreeRecursive(ref agentTree_, agent, ref rangeSq, agentTree_[node].right_);
                        }
                    }
                }
                else
                {
                    if (distSqRight < rangeSq)
                    {
                        queryAgentTreeRecursive(ref agentTree_, agent, ref rangeSq, agentTree_[node].right_);

                        if (distSqLeft < rangeSq)
                        {
                            queryAgentTreeRecursive(ref agentTree_, agent, ref rangeSq, agentTree_[node].left_);
                        }
                    }
                }

            }
        }
    }
}
