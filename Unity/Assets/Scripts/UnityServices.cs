using UnityEngine;
using Entitas;
using Entitas.Unity;
using Lockstep.Core.Interfaces;          

public interface IEventListener
{
    void RegisterListeners(IEntity entity);
}

public interface IComponentSetter
{
    void SetComponent(GameEntity entity);
}

public class UnityGameService : IGameService
{
    private readonly RTSEntityDatabase _entityDatabase;

    public UnityGameService(RTSEntityDatabase entityDatabase)
    {
        _entityDatabase = entityDatabase;
    }

    public void LoadEntity(GameEntity entity, int configId)
    {
        var viewGo = Object.Instantiate(_entityDatabase.Entities[configId]).gameObject;
        if (viewGo != null)
        {
            viewGo.Link(entity);

            var componentSetters = viewGo.GetComponents<IComponentSetter>();
            foreach (var componentSetter in componentSetters)
            {
                componentSetter.SetComponent(entity);
                Object.Destroy((MonoBehaviour) componentSetter);
            }

            var eventListeners = viewGo.GetComponents<IEventListener>();
            foreach (var listener in eventListeners)
            {
                listener.RegisterListeners(entity);
            }
        }
    }   
}

public class UnityLogger : ILogService
{
    public void Warn(string message)
    {
        Debug.LogWarning(message);
    }
}