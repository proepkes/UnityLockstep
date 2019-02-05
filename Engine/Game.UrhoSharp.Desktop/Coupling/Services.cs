using Lockstep.Game.Services;
using Urho;
using Urho.Resources;

namespace Game.UrhoSharp.Desktop
{
    public class ViewService : IViewService
    {
        private readonly Node _parent;
        private readonly ResourceCache _cache;

        public ViewService(ResourceCache cache, Node parent)
        {
            _cache = cache;
            _parent = parent;
        }
        public void LoadView(GameEntity entity, int configId)
        {
            Spawn(new Vector3(new Vector3((float)entity.position.value.X, 0, (float)entity.position.value.Y)));   
        }

        public void DeleteView(uint entityId)
        {
            
        }

        private void Spawn(Vector3 pos)
        {
            var jackNode = _parent.CreateChild("Jack");
            jackNode.Position = pos;
            var modelObject = jackNode.CreateComponent<AnimatedModel>();
            modelObject.Model = _cache.GetModel("Models/Jack.mdl");
            modelObject.SetMaterial(_cache.GetMaterial("Materials/Jack.xml"));
            modelObject.CastShadows = true;
        }
    }
}
