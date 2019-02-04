using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.DefaultServices;
using Lockstep.Core.DefaultServices.Navigation;
using Lockstep.Core.Interfaces;

namespace Lockstep.Core
{
    public class Services
    {
        private readonly Dictionary<string, IService> _instances = new Dictionary<string, IService>();
        private readonly Dictionary<string, IService> _defaults = new Dictionary<string, IService>();

        public Services()
        {
            RegisterDefault(new DefaultHashService());
            RegisterDefault(new DefaultViewService());
            RegisterDefault(new DefaultNavigationService());         
            RegisterDefault(new DefaultSnapshotIndexService());
            RegisterDefault(new DefaultIDebugService());
        }

        public void Register(IService instance)
        {
            foreach (var name in GetInterfaceNames(instance))
            {
                _instances.Add(name, instance);
            }
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

        private void RegisterDefault(IService instance)
        {
            foreach (var name in GetInterfaceNames(instance))
            {
                _defaults.Add(name, instance);
            }
        }

        private string[] GetInterfaceNames(object instance)
        {
             return instance.GetType().FindInterfaces((type, criteria) =>
                type.GetInterfaces()
                    .Any(t => t.FullName == typeof(IService).FullName), instance)
                    .Select(type => type.FullName).ToArray();
        }
    }
}