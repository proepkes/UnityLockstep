using ECS;
using UnityEngine;
using Entitas;
using Entitas.Unity;           

public interface IViewController
{
    Vector2 Position { get; set; }
    Vector2 Scale { get; set; }
    bool Active { get; set; }
    void InitializeView(Contexts contexts, IEntity Entity);
    void DestroyView();
}

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

    public void ApplyEntity(GameEntity entity, int configId)
    {
        var viewGo = GameObject.Instantiate(_entityDatabase.Entities[configId]).gameObject;
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