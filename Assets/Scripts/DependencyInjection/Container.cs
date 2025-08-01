using System;
using System.Collections.Generic;
using UnityEngine;

namespace DependencyInjection
{
    public class Container 
    {
        public Container ParentContainer;
        
        protected readonly Dictionary<Type, IProviderWrapper> _providers = new Dictionary<Type, IProviderWrapper>();
        
        public ResolveWrapper RegisterUnitySingleton<T>( bool persistent = false) where T : Component
        {
            RegisterProvider(new UnitySingletonProvider<T>(persistent));
            return new ResolveWrapper(this, typeof(T));
        }
        
        public virtual void RegisterInstance<T>(T obj) where T : class
        { 
            RegisterProvider(new InstanceProvider<T>(obj));
        }
        
        public void RegisterInstance(object obj, Type type)
        {
            RegisterTypeProvider(type, new InstanceProviderNonGeneric(obj));
        }
        
        public ResolveWrapper RegisterSingleton<T>() where T : class
        {
            RegisterProvider<T, T>(new SingletonProvider<T>());
            return new ResolveWrapper(this, typeof(T));
        }
        
        public ResolveWrapper RegisterSingleton<TBase, TDerived>() where TDerived : class, TBase
        {
            RegisterProvider<TBase, TDerived>(new SingletonProvider<TDerived>());
            return new ResolveWrapper(this, typeof(TBase));
        }
        
        private void RegisterProvider<T>(IObjectProvider<T> provider) where T : class
        {
            _providers[typeof(T)] = new ProviderWrapper<T>(provider);
        }
        private void RegisterProvider<TBase, TDerived>(IObjectProvider<TDerived> provider) where TDerived : class, TBase
        {
            _providers[typeof(TBase)] = new ProviderWrapper<TDerived>(provider);
        }
        
        public void RegisterTypeProvider(Type type, IProviderWrapper provider) {
            _providers[type] = provider;
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }
        
        public object Resolve(Type type)
        {
            if (!_providers.TryGetValue(type, out IProviderWrapper provider))
            {
                if (ParentContainer != null)
                {
                    return ParentContainer.Resolve(type);
                }
            }

            if (provider == null)
            {
                //TODO: add exception and handler
                Debug.LogError($"No provider for type {type}");
                return null;
            }
            return GetObject(this, provider, type);
        }

        protected virtual object GetObject(Container container, IProviderWrapper provider, Type type)
        {
            return provider.GetObject(this);
        }
    }
}