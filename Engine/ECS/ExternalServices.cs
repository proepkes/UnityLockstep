using BEPUutilities;
using ECS.Data;
using Entitas;        

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

public interface IPathfindingService : IService
{
    void AddAgent(GameEntity entity, Vector2 position);
    void UpdateAgents(GameEntity[] entities);
    Vector2 GetAgentPosition(int agentId);
}
      
public interface ILogger : IService
{
    void Warn(string message);
}