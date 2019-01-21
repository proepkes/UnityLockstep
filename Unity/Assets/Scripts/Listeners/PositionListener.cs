using Entitas;
using UnityEngine;

public class PositionListener : MonoBehaviour, IEventListener, IPositionListener
{
    private GameEntity _entity;   

    public void RegisterListeners(IEntity entity)
    {
        _entity = (GameEntity)entity;
        _entity.AddPositionListener(this);
    }

    public void OnPosition(GameEntity entity, BEPUutilities.Vector2 newPosition)
    {
        transform.position = new Vector3((float) newPosition.X, 1, (float) newPosition.Y);
    }
}