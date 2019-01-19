using System;
using System.Collections.Generic;
using ECS.Systems.Input;
using ECS.Systems.Navigation;

namespace ECS.Features
{
    public sealed class InputFeature : Feature
    {
        public InputFeature(Contexts contexts, ServiceContainer serviceContainer)
        {    
            Add(new EmitInputSystem(contexts, serviceContainer.Get<IParseInputService>()));
            Add(new OnInputCreateGameEntity(contexts));
            Add(new OnGameEntityLoadAsset(contexts, serviceContainer.Get<IViewService>()));
        }
    }

    public class ServiceContainer
    {
        private readonly Dictionary<string, IService> _instances = new Dictionary<string, IService>();

        public ServiceContainer Register<T>(T instance) where T : IService
        {
            var key = typeof(T).FullName;
            if (key != null)
            {
                _instances.Add(key, instance);
            }

            return this;
        }

        public T Get<T>() where T : IService
        {
            var key = typeof(T).FullName;
            if (key == null || !_instances.ContainsKey(key))
            {
                return default(T);
            }

            return (T) _instances[key];
        }
    }
}
