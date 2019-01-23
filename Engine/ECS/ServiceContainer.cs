using System.Collections.Generic;
using ECS.DefaultServices;
using ECS.DefaultServices.Navigation;         

namespace ECS
{
    public class ServiceContainer
    {
        private readonly Dictionary<string, IService> _instances = new Dictionary<string, IService>();
        private readonly Dictionary<string, IService> _defaults = new Dictionary<string, IService>();

        public ServiceContainer()
        {
            _defaults.Add(typeof(IHashService).FullName, new DefaultHashService());
            _defaults.Add(typeof(INavigationService).FullName, new DefaultNavigationService());
            _defaults.Add(typeof(IGameService).FullName, new DefaultGameService());
            _defaults.Add(typeof(IDataSource).FullName, new DefaultDataSource());
        }

        public ServiceContainer Register<T>(T instance) where T : class, IService
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
            if (key == null)
            {
                return default(T);
            }

            if (!_instances.ContainsKey(key))
            {
                if (_defaults.ContainsKey(key))
                {
                    return (T)_defaults[key];
                }

                return default(T);
            }

            return (T) _instances[key];
        }
    }
}