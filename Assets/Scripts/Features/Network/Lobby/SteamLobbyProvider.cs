using System;
using Steamworks;
using UnityEngine;

namespace Network.Lobby
{
    public class SteamLobbyProvider
    {
        public event Action<LobbyCreated_t>  OnLobbyCreatedEvent;
        public event Action<LobbyMatchList_t>  OnLobbyListLoadedEvent;
        public event Action<LobbyChatUpdate_t>  OnLobbyChatUpdateEvent;
        public event Action<CSteamID>  OnLobbyEnteredEvent;
        
        protected Callback<LobbyCreated_t> _LobbyCreated;
        protected Callback<LobbyMatchList_t> _LobbyMatchList;
        protected Callback<LobbyEnter_t> _LobbyEntered;
        protected Callback<LobbyChatUpdate_t> _LobbyChatUpdate;
        
        public SteamLobbyProvider()
        {
            if (!SteamAPI.Init())
            {
                Debug.LogError("Steam API Init failed!");
                return;
            }

            _LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            _LobbyMatchList = Callback<LobbyMatchList_t>.Create(OnLobbyList);
            _LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            _LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            Debug.Log("STEAM::OnLobbyCreated");
            OnLobbyCreatedEvent?.Invoke(callback);
        }

        public void HostLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
        }

        public void SetLobbyData(CSteamID lobbyId, string key, string value)
        {
            SteamMatchmaking.SetLobbyData(lobbyId, key, value);
        }

        public void LoadLobbyList(string filterKey, string filterValue)
        {
            SteamMatchmaking.AddRequestLobbyListResultCountFilter(50);
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.AddRequestLobbyListStringFilter(filterKey, filterValue, ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.RequestLobbyList();
        }

        public void JoinLobby(CSteamID lobbyId)
        {
            SteamMatchmaking.JoinLobby(lobbyId);
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            var lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            OnLobbyEnteredEvent?.Invoke(lobbyID);
        }
        
        private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            OnLobbyChatUpdateEvent?.Invoke(callback);
        }

        private void OnLobbyList(LobbyMatchList_t param)
        {
            OnLobbyListLoadedEvent?.Invoke(param);
        }
    }
}