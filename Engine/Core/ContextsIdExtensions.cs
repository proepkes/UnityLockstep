namespace Lockstep.Core
{
    public static class ContextsIdExtensions
    {
        public static void SubscribeId(this Contexts contexts)
        {         
            contexts.game.OnEntityCreated += (context, entity) => ((GameEntity)entity).AddId(entity.creationIndex);
        }
    }
}
