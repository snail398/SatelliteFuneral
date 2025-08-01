using System;
using System.Linq;

namespace DependencyInjection
{
    public class ResolveWrapper
    {
        
        private readonly Container _container;
        private Type _type;
        
        public ResolveWrapper(Container container, Type type)
        {
            _container = container;
            _type = type;
        }

        public void ResolveAndLoad()
        {
            (_container.Resolve(_type) as ILoadableService)?.Load();
        }
        
        public void Resolve()
        {
            _container.Resolve(_type);
        }
        
        public T Resolve<T>()
        {
            return (T)_container.Resolve(_type);
        }
        public void RegisterInstanceInterfaces(params Type[] excludedInterfacesTypes)
        {
            var instance = _container.Resolve(_type);
            instance.GetType().GetInterfaces()
                .Where(_ => !excludedInterfacesTypes.Contains(_))
                .ToList()
                .ForEach(_ => {
                    _container.RegisterTypeProvider(_, new InstanceProviderNonGeneric(instance));
                });
        }
    }
}