using DependencyInjection;
using UnityEngine;

namespace CompositionRoot
{
    public class CompositionRoot
    {
        public static Container GlobalContainer { get; } = new DisposableContainer();
        public static Container Container { get; private set; } = GlobalContainer;
        
        private Context _currentContext;

        public CompositionRoot() {
            GlobalContainer.RegisterInstance(this);
            GlobalContainer.RegisterInstance(GlobalContainer);
        }

        public async void SwitchContext(Context context)
        {
            if (_currentContext != null)
            {
                 await _currentContext.UnloadContext();
            }

            TryToDisposeCurrentContainer();
            _currentContext = context;
            Container = new DisposableContainer() { ParentContainer = GlobalContainer };
            Container.RegisterInstance(Container);
            await _currentContext.LoadContext();
            Debug.LogError($"switched to {context.GetType().Name}");
        }
        
        private void TryToDisposeCurrentContainer() {
            if (Container != GlobalContainer && Container is DisposableContainer container)
                container.Dispose();
        }
    }
}