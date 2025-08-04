using System.Threading.Tasks;
using Client;
using Network.Lobby;
using Network.Transport;
using Shared;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameServer = Server.GameServer;

namespace CompositionRoot
{
    public class GameContext : Context
    {

        private GameHostingType _GameHostingType;

        public GameContext(GameHostingType gameHostingType)
        {
            _GameHostingType = gameHostingType;
        }

        public override async Task LoadContext()
        {
            Debug.LogError("Load Game Context");
            
            await SceneManager.LoadSceneAsync("Game");
            var lobbyServer = CompositionRoot.Container.Resolve<GameLobbyServer>();
            if (_GameHostingType == GameHostingType.Host)
            {
                lobbyServer.HostLobby();
            }
            else
            {
                lobbyServer.JoinLobby();
            }
            await lobbyServer.WaitForEnterLobby();
            
            CompositionRoot.Container.RegisterSingleton<MessageProcessor>().Resolve();
            CompositionRoot.Container.RegisterSingleton<MessageDataSerializer>().Resolve();
            if (_GameHostingType == GameHostingType.Host)
            {
                var localServerProvider = (LocalGameServerProvider)CompositionRoot.Container.RegisterSingleton<IServerProvider, LocalGameServerProvider>().Resolve<IServerProvider>();
                CompositionRoot.Container.RegisterInstance(localServerProvider);
                CompositionRoot.Container.RegisterSingleton<GameServer>().ResolveAndLoad();
                CompositionRoot.Container.RegisterSingleton<IMessageSender, LocalMessageSender>().ResolveAndLoad();
                
                var gameServer = CompositionRoot.Container.Resolve<GameServer>();
                
                //TODO remove dependency on SteamNetworking
                gameServer.CreateGame(SteamUser.GetSteamID());
            }
            else
            {
                CompositionRoot.Container.RegisterSingleton<IServerProvider, RemoteGameServerProvider>().ResolveAndLoad();
                CompositionRoot.Container.RegisterSingleton<IMessageSender, RemoteMessageSender>().ResolveAndLoad();
                
                var messageSender = CompositionRoot.Container.Resolve<IMessageSender>();
                messageSender.SendMessage(new WelcomeMessage()
                {
                    Data = 1,
                });
            }
            
            CompositionRoot.Container.RegisterSingleton<SynchronizationService>().Resolve();
            CompositionRoot.Container.RegisterSingleton<SpawnService>().ResolveAndLoad();
            
            Debug.LogError("After Load context");
        }

        public override async Task UnloadContext()
        {
            Debug.LogError("Unload Game Context");
        }
    }
}

public enum GameHostingType
{
    Host,
    Client,
}