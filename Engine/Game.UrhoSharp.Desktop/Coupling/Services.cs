using System.Collections.Generic;
using Lockstep.Core.State.Game;
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
        public void Instantiate(GameEntity e, int configId)
        {                    
            Application.InvokeOnMain(() =>
            {               
                var node = Spawn(new Vector3((float)e.position.value.X, 0, (float)e.position.value.Y));
                node.GetComponent<PositionListener>().RegisterListeners(e);

                e.AddRadius(1);
                e.AddMaxSpeed(1);
                e.AddAgent(BEPUutilities.Vector2.Zero, BEPUutilities.Vector2.Zero, 1, 15, 10, 5, new List<Line>());

                _linkedEntities.Add(e.localId.value, node);
            });
        }

        public void Destroy(uint entityId)
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
