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

public interface ICommandService : IService
{
    void Process(InputContext context, Command command);
}

public interface IViewService : IService
{
    // create a view from a premade asset (e.g. a prefab)
    void LoadAsset(
        Contexts contexts,
        IEntity entity,
        string assetName);
}