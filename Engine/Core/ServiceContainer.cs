using System.Collections.Generic;
using System.Linq;
using Lockstep.Core.Services;

namespace Lockstep.Game
{
    public class ServiceContainer
    {
        private readonly Dictionary<string, IService> _instances = new Dictionary<string, IService>();

        /// <summary>
        /// Registers an instance for all of its implemented IService(s) if no other instance for that IService exists
        /// </summary>
        /// <param name="instance"></param>
        public void TryRegister(IService instance)
        {
            foreach (var name in GetInterfaceNames(instance))
            {
                if(!_instances.ContainsKey(name))
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
                return default(T);
            }

            return (T) _instances[key];
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