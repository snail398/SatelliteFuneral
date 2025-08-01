using Network.Lobby;
using Shared;
using Utils;

namespace CompositionRoot
{
    public class RootAppInitializer : AppInitializationProcessor
    {
        
        public override void InitializeApplication()
        {
            var compositionRoot = new CompositionRoot();

            CompositionRoot.Container.RegisterUnitySingleton<UnityEventProvider>(true).Resolve();
            CompositionRoot.Container.RegisterUnitySingleton<SteamManager>(true).Resolve();
            CompositionRoot.Container.RegisterSingleton<LobbyData>().Resolve();
            CompositionRoot.Container.RegisterSingleton<GameLobbyServer>().Resolve();
            CompositionRoot.Container.RegisterSingleton<ITimerProvider, CoroutineTimerProvider>().Resolve();
            compositionRoot.SwitchContext(new MenuContext());
        }
    }
}