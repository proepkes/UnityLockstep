
using System.Collections.Generic;
using Entitas;

public class AddControllableSystem   : ReactiveSystem<GameEntity>
{
    public AddControllableSystem(Contexts contexts) : base(contexts.game)
    {
        
    }
    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.Team.Added());
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.hasTeam;
    }

    protected override void Execute(List<GameEntity> entities)
    {                                                
    }
}       