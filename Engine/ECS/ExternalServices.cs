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
    void UpdateAgents();
}