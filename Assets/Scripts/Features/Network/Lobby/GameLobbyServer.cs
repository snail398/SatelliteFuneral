using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DependencyInjection;
using Steamworks;
using UnityEngine;

namespace Network.Lobby
{
    public class GameLobbyServer : ILoadableService, IUnloadableService
    {
        public event Action<CSteamID> OnUserJoin; 
        public event Action<CSteamID> OnUserLeft; 
        
        private const string FILTER_KEY = "filter_key";
        private const string FILTER_VALUE = "filter_value";
        
        private readonly SteamLobbyProvider _SteamLobbyProvider;
        private readonly Container _Container;
        
        CSteamID _LobbyID = CSteamID.Nil;
        private bool _InLobby = false;
        private List<CSteamID> _UsersInLobby = new List<CSteamID>();

        public List<CSteamID> UsersInLobby => _UsersInLobby;

        public GameLobbyServer(Container container)
        {
            _Container = container;
            _SteamLobbyProvider = new SteamLobbyProvider();
            _SteamLobbyProvider.OnLobbyCreatedEvent += OnLobbyCreated;
            _SteamLobbyProvider.OnLobbyListLoadedEvent += OnLobbyListLoaded;
            _SteamLobbyProvider.OnLobbyChatUpdateEvent += OnLobbyChatUpdate;
            _SteamLobbyProvider.OnLobbyEnteredEvent += OnLobbyEntered;
        }

        void ILoadableService.Load() { }
        
        public void HostLobby()
        {
            _SteamLobbyProvider.HostLobby();
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Failed to create lobby!");
                return;
            }

            _LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            _SteamLobbyProvider.SetLobbyData(_LobbyID, "host_id", SteamUser.GetSteamID().ToString());
            _SteamLobbyProvider.SetLobbyData(_LobbyID, FILTER_KEY, FILTER_VALUE);
            Debug.Log($"Lobby created. lobby id: {_LobbyID}");
        }

        public void JoinLobby()
        {
            _SteamLobbyProvider.LoadLobbyList(FILTER_KEY, FILTER_VALUE);   
        }

        private void OnLobbyListLoaded(LobbyMatchList_t callback)
        {
            for (int i = 0; i < callback.m_nLobbiesMatching; i++)
            {
                CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                string code = SteamMatchmaking.GetLobbyData(lobbyId, FILTER_KEY);
                if (code == FILTER_VALUE)
                {
                    _SteamLobbyProvider.JoinLobby(lobbyId);
                    return;
                }
            }

            Debug.LogWarning("Lobby with code " + 228 + " not found.");
        }

        private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            CSteamID changed = new CSteamID(callback.m_ulSteamIDUserChanged);
            EChatMemberStateChange change = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
            if (change == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
            {
                Debug.Log("Player joined: " + changed);
                _UsersInLobby.Add(changed);
                OnUserJoin?.Invoke(changed);
            }
            else if (change == EChatMemberStateChange.k_EChatMemberStateChangeLeft ||
                     change == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
            {
                Debug.Log("Player left: " + changed);
                _UsersInLobby.Remove(changed);
                OnUserLeft?.Invoke(changed);
            }
        }
        

        private void OnLobbyEntered(CSteamID lobbyId)
        {
            _InLobby = true;
            string hostIdStr = SteamMatchmaking.GetLobbyData(lobbyId, "host_id");
            var lobbyData = _Container.Resolve<LobbyData>();
            lobbyData.LobbyId = lobbyId;
            lobbyData.HostId = new CSteamID(ulong.Parse(hostIdStr));
            Debug.LogError($"hostIdStr lobby id: {hostIdStr}");
            if (SteamUser.GetSteamID().ToString() == hostIdStr)
            {
                Debug.Log("Host entered lobby.");
                var steamId = new CSteamID(ulong.Parse(hostIdStr));
                _UsersInLobby.Add(steamId);
                OnUserJoin?.Invoke(steamId);
            }
        }

        public async Task WaitForEnterLobby()
        {
            if (_InLobby)
                return;
            while (!_InLobby)
            {
                await Task.Delay(1000);
                Debug.Log("Waiting for lobby...");
            }
        }

        void IUnloadableService.Unload() { }
    }
}