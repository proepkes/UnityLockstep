using BEPUutilities;
using ECS.Data;
using Entitas;

namespace ECS
{
    public interface IService
    {

    }
                    
    public interface IGridService : IService
    {
        Vector2 GetWorldSize();
        Vector2 GetCellSize();        
    }

    public interface IParseInputService : IService
    {
        void Parse(InputContext context, SerializedInput serializedInput);
    }

    public interface IViewService : IService
    {                                                        
        void LoadAsset(Contexts contexts, IEntity entity, string assetName);
    }

    public interface INavigationService : IService
    {
        void AddAgent(int id, Vector2 position);

        void UpdateDestination(int[] agentIds, Vector2 newDestination);
                                                                 
        void UpdateAgents();

        Vector2 GetAgentPosition(int agentId);
    }

    public interface IHashService : IService
    {
        long GetHashCode(GameEntity entity);
    }
      
    public interface ILogger : IService
    {
        void Warn(string message);
    }
}