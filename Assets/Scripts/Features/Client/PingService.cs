using System.Collections.Generic;
using Client.UI;
using Shared;
using UnityEngine;

namespace Client
{
    public class PingService : ISnapshotDataUpdateReceiver<List<PingSnapshot>>
    {
        private readonly UISceneSettings _UISceneSettings;

        private readonly Dictionary<ulong, PingItemView> _PingItemViews = new Dictionary<ulong, PingItemView>();
        private readonly HashSet<ulong> _CurrentActivePlayers = new HashSet<ulong>();
        private readonly List<ulong> _RemoveList = new List<ulong>();
        
        public PingService(UISceneSettings uiSceneSettings)
        {
            _UISceneSettings = uiSceneSettings;
        }

        public void ReceiveSnapshotDataUpdate(List<PingSnapshot> data, long timestamp)
        {
            if (data != null)
            {
                _CurrentActivePlayers.Clear();
                _RemoveList.Clear();
                foreach (var snapshot in data)
                {
                    _CurrentActivePlayers.Add(snapshot.SteamId);
                    if (!_PingItemViews.TryGetValue(snapshot.SteamId, out PingItemView pingItemView))
                    {
                        var prefab = Resources.Load<PingItemView>("UI/PingItem");
                        pingItemView = Object.Instantiate(prefab, _UISceneSettings.PingPanel.PingItemRoot);
                        _PingItemViews.Add(snapshot.SteamId, pingItemView);
                    }
                    pingItemView.SetData(snapshot.SteamId.ToString(), snapshot.Ping);
                }

                foreach (var kvp in _PingItemViews)
                {
                    if (!_CurrentActivePlayers.Contains(kvp.Key))
                    {
                        _RemoveList.Add(kvp.Key);
                    }
                }
                foreach (var removeId in _RemoveList)
                {
                    _PingItemViews.Remove(removeId);
                }
            }
        }
    }
}