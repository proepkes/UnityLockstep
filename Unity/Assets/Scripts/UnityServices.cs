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
    private readonly Transform _entityContainer;
    private readonly RTSEntityDatabase _entityDatabase;
    private readonly Dictionary<uint, GameObject> _linkedEntities = new Dictionary<uint, GameObject>();

    public UnityGameService(RTSEntityDatabase entityDatabase)
    {
        _entityDatabase = entityDatabase;
        _entityContainer = GameObject.Find("EntityContainer").transform;
    }

    public void LoadView(GameEntity entity, int configId)
    {
        //TODO: pooling    
        var viewGo = UnityEngine.Object.Instantiate(_entityDatabase.Entities[configId], _entityContainer).gameObject;
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

            _linkedEntities.Add(entity.localId.value, viewGo);
        }      
    }

    public void DeleteView(uint entityId)
    {                                            
        var viewGo = _linkedEntities[entityId];
        var eventListeners = viewGo.GetComponents<IEventListener>();
        foreach (var listener in eventListeners)
        {
            listener.UnregisterListeners();
        }

        _linkedEntities[entityId].Unlink();
        _linkedEntities[entityId].DestroyGameObject();
        _linkedEntities.Remove(entityId);      
    }
}         