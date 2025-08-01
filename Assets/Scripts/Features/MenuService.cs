using CompositionRoot;
using DependencyInjection;

namespace Features
{
    public class MenuService : ILoadableService, IUnloadableService
    {
        private readonly MenuSceneSettings _MenuSceneSettings;
        private readonly CompositionRoot.CompositionRoot _CompositionRoot;

        public MenuService(MenuSceneSettings menuSceneSettings, CompositionRoot.CompositionRoot compositionRoot)
        {
            _MenuSceneSettings = menuSceneSettings;
            _CompositionRoot = compositionRoot;
        }

        void ILoadableService.Load()
        {
            _MenuSceneSettings.HostGameButton.onClick.AddListener(OnHostButtonClicked);
            _MenuSceneSettings.JoinGameButton.onClick.AddListener(OnJoinButtonClicked);
        }
        
        private void OnHostButtonClicked()
        {
            _CompositionRoot.SwitchContext(new GameContext(GameHostingType.Host));
        }
        
        private void OnJoinButtonClicked()
        {
            _CompositionRoot.SwitchContext(new GameContext(GameHostingType.Client));
        }

        void IUnloadableService.Unload() { }
    }
}