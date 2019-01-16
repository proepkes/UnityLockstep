using System.Collections.Generic;
using Entitas;

public class LoadAssetSystem : ReactiveSystem<GameEntity>
{
    readonly Contexts _contexts;
    readonly IViewService _viewService;                     

    public LoadAssetSystem(Contexts contexts, IViewService viewService) : base(contexts.game)
    {
        _contexts = contexts;
        _viewService = viewService;
    }                            

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.Asset);
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.hasAsset;
    }

    protected override void Execute(List<GameEntity> entities)
    {
        foreach (var e in entities)
        {                                                 
             _viewService.LoadAsset(_contexts, e, e.asset.name);   
        }
    }
    
    
}