using Urho;
using Vector2 = BEPUutilities.Vector2;

namespace Game.UrhoSharp.Desktop.Coupling
{
    public class PositionListener : Component, IPositionListener
    {
        private GameEntity _entity;

        public void RegisterListeners(GameEntity entity)
        {
            _entity = entity;
            _entity.AddPositionListener(this);
        }
        public void UnregisterListeners()
        {
            _entity.RemovePositionListener(this);
        }

        public void OnPosition(GameEntity entity, Vector2 value)
        {
            Node.SetWorldPosition(new Vector3((float)value.X, 0, (float)value.Y));
        }
    }
}
