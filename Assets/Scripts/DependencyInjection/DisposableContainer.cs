using System;
using System.Collections.Generic;

namespace DependencyInjection
{
    public class DisposableContainer : Container, IDisposable
    {
        private readonly List<(object, Type)> _resolvedObjects = new List<(object, Type)>();
        
        public override void RegisterInstance<T>(T obj) {
            TryAddResolvedObject(obj, typeof(T));
            base.RegisterInstance<T>(obj);
        }

        protected override object GetObject(Container container, IProviderWrapper provider, Type type)
        {
            var result = base.GetObject(container, provider, type);
            TryAddResolvedObject(result, type);
            return result;
        }

        private void TryAddResolvedObject(object resolvedObject, Type key) {
            if (resolvedObject != null && (resolvedObject is IUnloadableService || resolvedObject is IDisposable)) {
                var uniqueObject = true;
                foreach (var (obj, type) in _resolvedObjects) {
                    if (obj.Equals(resolvedObject)) {
                        uniqueObject = false;
                        break;
                    }
                }
                if (uniqueObject)
                    _resolvedObjects.Add((resolvedObject, key));
            }
        }

        public void Dispose()
        {
            foreach (var (obj, key) in _resolvedObjects) {
                if (obj == this)
                    continue;
                (obj as IUnloadableService)?.Unload();
                (obj as IDisposable)?.Dispose();
            }
            _resolvedObjects.Clear();
        }
    }
}