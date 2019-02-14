using System;
using System.Collections.Generic;
using UnityEngine;       
using Entitas.Unity;
using Entitas.VisualDebugging.Unity;               
using Lockstep.Game.Interfaces;

public interface IEventListener
{
    void RegisterListeners(GameEntity entity);
    void UnregisterListeners();

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
        var viewGo = UnityEngine.Object.Instantiate(_entityDatabase.Entities[configId]).gameObject;
        if (viewGo != null)
        {
            viewGo.Link(entity);

            var componentSetters = viewGo.GetComponents<IComponentSetter>();
            foreach (var componentSetter in componentSetters)
            {
                componentSetter.SetComponent(entity);
                UnityEngine.Object.Destroy((MonoBehaviour)componentSetter);
            }

            var eventListeners = viewGo.GetComponents<IEventListener>();
            foreach (var listener in eventListeners)
            {
                listener.RegisterListeners(entity);
            }

            linkedEntities.Add(entity.localId.value, viewGo);
        }      
    }

    public void DeleteView(uint entityId)
    {                                            
        var viewGo = linkedEntities[entityId];
        var eventListeners = viewGo.GetComponents<IEventListener>();
        foreach (var listener in eventListeners)
        {
            listener.UnregisterListeners();
        }

        linkedEntities[entityId].Unlink();
        linkedEntities[entityId].DestroyGameObject();
        linkedEntities.Remove(entityId);      
    }
}         