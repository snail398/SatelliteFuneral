using System;
using System.Linq;
using System.Reflection;

namespace DependencyInjection
{
    public class SingletonProvider<T> : IObjectProvider<T> where T : class
    {
        private bool _initialized;
        private T _instance;
		
        public T GetObject(Container container)
        {
            if (!_initialized)
            {
                var ctor = typeof(T)
                    .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();
                var parameters = ctor.GetParameters()
                    .Select(p => container.Resolve(p.ParameterType))
                    .ToArray();
                _instance  = (T)Activator.CreateInstance(typeof(T), parameters);
                _initialized = true;
            }
            return _instance;
        }
        
        public object GetObject(Container container, Type type)
        {
            if (!_initialized)
            {
                var ctor = type
                    .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();
                var parameters = ctor.GetParameters()
                    .Select(p => container.Resolve(p.ParameterType))
                    .ToArray();
                _instance  = (T)Activator.CreateInstance(type, parameters);
                _initialized = true;
            }
            return _instance;
        }
    }
}