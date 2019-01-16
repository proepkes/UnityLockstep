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

public class UnityViewService : IViewService
{                                                   
    public void LoadAsset(Contexts contexts, IEntity entity, string assetName)
    {
        var viewGo = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/" + assetName));
        if (viewGo != null)
        {
            viewGo.Link(entity);
            var viewController = viewGo.GetComponent<IViewController>();
            viewController?.InitializeView(contexts, entity);
                                                                                   
            var eventListeners = viewGo.GetComponents<IEventListener>();
            foreach (var listener in eventListeners)
            {
                listener.RegisterListeners(entity);
            }
        }
    }
}