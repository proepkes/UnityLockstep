using Entitas;
using FixMath.NET;
using Lockstep.Common.Logging;
using Lockstep.Core.State;
using Lockstep.Game.Features.Navigation.RVO.Algorithm;

namespace Lockstep.Core.Logic.Systems.Game
{
    public class BuildKdTree : IInitializeSystem
    {
        private const int MAX_LEAF_SIZE = 10;

        private readonly GameContext _gameContext;
        private readonly IGroup<GameEntity> _entities;

        public BuildKdTree(Contexts contexts)
        {
            _gameContext = contexts.game;
            _entities = _gameContext.GetGroup(GameMatcher.LocalId);
        }

        public void Initialize()
        {
            _gameContext.SetKdTree(true, new AgentTreeNode[0]);
            _gameContext.OnEntityCreated += buildTree;
            _gameContext.OnEntityWillBeDestroyed += buildTree;
        }

        private void buildTree(IContext context, IEntity entity)
        {
            if (!((GameEntity)entity).hasLocalId)
            {
                return;
            }

            var kdTree = _gameContext.kdTree;

            kdTree.AgentTree = new AgentTreeNode[2 * _entities.count];

            for (int i = 0; i < kdTree.AgentTree.Length; ++i)
            {
                kdTree.AgentTree[i] = new AgentTreeNode();
            }

            if (_entities.count > 0)
            {
                buildAgentTreeRecursive(ref kdTree.AgentTree, _entities.GetEntities(), 0, _entities.count, 0);
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
    }
}
