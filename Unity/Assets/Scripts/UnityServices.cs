using System.Collections.Generic;
using UnityEngine;       
using Entitas.Unity;
using Entitas.VisualDebugging.Unity;               
using Lockstep.Game.Interfaces;

public interface IEventListener
{
    void RegisterListeners(GameEntity entity);
    void DeregisterListeners();

}

public interface IEntityConfigurator
{
    void Configure(GameEntity entity);
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

    public void Instantiate(GameEntity entity, int databaseId)
    {
        //TODO: pooling    
        var viewGo = Object.Instantiate(_entityDatabase.Entities[databaseId], _entityContainer).gameObject;
        if (viewGo != null)
        {
            viewGo.Link(entity);

            var configurators = viewGo.GetComponents<IEntityConfigurator>();
            foreach (var configurator in configurators)
            {
                configurator.Configure(entity);
                Object.Destroy((MonoBehaviour)configurator);
            }

            var eventListeners = viewGo.GetComponents<IEventListener>();
            foreach (var listener in eventListeners)
            {
                listener.RegisterListeners(entity);
            }

            _linkedEntities.Add(entity.localId.value, viewGo);
        }      
    }

    public void Destroy(uint entityId)
    {                                            
        var viewGo = _linkedEntities[entityId];
        var eventListeners = viewGo.GetComponents<IEventListener>();
        foreach (var listener in eventListeners)
        {
            listener.DeregisterListeners();
        }

        _linkedEntities[entityId].Unlink();
        _linkedEntities[entityId].DestroyGameObject();
        _linkedEntities.Remove(entityId);      
    }
}         