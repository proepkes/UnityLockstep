namespace Lockstep.Core.Interfaces
{
    public interface IHashService : IService
    {  
        long CalculateHashCode(GameEntity[] hashableEntities);
    }
}