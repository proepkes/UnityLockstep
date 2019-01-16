using System.Collections.Generic;
using Entitas;

public class LoadAssetSystem : ReactiveSystem<GameEntity>, IInitializeSystem
{
    readonly Contexts _contexts;
    IViewService _viewService;                     

    public LoadAssetSystem(Contexts contexts) : base(contexts.game)
    {
        _contexts = contexts;
    }

    public void Initialize()
    {
        // grab the view service instance from the meta context
        _viewService = _contexts.service.viewService.instance;
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.Asset.Added());
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