using System.Collections.Generic;
using UnityEngine;
using Entitas;
using Entitas.Unity;
using Entitas.VisualDebugging.Unity;
using Lockstep.Core.Interfaces;          

public interface IEventListener
{
    void RegisterListeners(IEntity entity);
}

public interface IComponentSetter
{
    void SetComponent(GameEntity entity);
}

public class UnityGameService : IViewService
{
    private readonly RTSEntityDatabase _entityDatabase;
    private Dictionary<uint, GameObject> linkedEntities = new Dictionary<uint, GameObject>();

    public UnityGameService(RTSEntityDatabase entityDatabase)
    {
        _entityDatabase = entityDatabase;
    }

    public void LoadView(GameEntity entity, int configId)
    {
        //TODO: pooling
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

            linkedEntities.Add(entity.id.value, viewGo);
        }
    }

    public void DeleteView(uint entityId)
    {
        linkedEntities[entityId].Unlink();
        linkedEntities[entityId].DestroyGameObject();
        linkedEntities.Remove(entityId);
    }
}

public class UnityLogger : ILogService
{
    public void Warn(object message)
    {
        Debug.LogWarning(message);
    }
}