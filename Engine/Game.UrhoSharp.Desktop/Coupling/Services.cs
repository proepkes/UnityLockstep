using System.Collections.Generic;
using Lockstep.Game.Interfaces;
using Urho;
using Urho.Resources;

namespace Game.UrhoSharp.Desktop.Coupling
{
    public class ViewService : IViewService
    {
        private readonly Node _parent;
        private readonly ResourceCache _cache;

        private readonly Dictionary<uint, Node> _linkedEntities = new Dictionary<uint, Node>();

        public ViewService(ResourceCache cache, Node parent)
        {
            _cache = cache;
            _parent = parent;
        }
        public void LoadView(GameEntity entity, int configId)
        {
            var node = Spawn(new Vector3((float) entity.position.value.X, 0, (float) entity.position.value.Y));
            node.GetComponent<PositionListener>().RegisterListeners(entity);

            entity.isNavigable = true;

            _linkedEntities.Add(entity.localId.value, node);   
        }

        public void DeleteView(uint entityId)
        {
            var viewGo = _linkedEntities[entityId];      
            viewGo.GetComponent<PositionListener>().UnregisterListeners();

            _linkedEntities[entityId].Remove();             
            _linkedEntities.Remove(entityId);

        }

        private Node Spawn(Vector3 pos)
        {
            var jackNode = _parent.CreateChild("Jack");
            jackNode.Position = pos;
            var modelObject = jackNode.CreateComponent<AnimatedModel>();
            modelObject.Model = _cache.GetModel("Models/Jack.mdl");
            modelObject.SetMaterial(_cache.GetMaterial("Materials/Jack.xml"));
            modelObject.CastShadows = true;
            jackNode.CreateComponent<PositionListener>();
            
            return jackNode;
        }
    }
}
