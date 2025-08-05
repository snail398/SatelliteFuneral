using System;
using System.Collections.Generic;
using Network.Transport;
using Steamworks;
using Unity.Mathematics;
using UnityEngine;
using Utils;

namespace Client
{
    public class UnitController : IDisposable
    {
        private ulong _SteamId;
        private UnitView _View;
        private IMessageSender _MessageSender;
        private IServerProvider _ServerProvider;
        private UnityEventProvider _UnityEventProvider;
        private bool _IsLocal;
        public float moveSpeed = 5f;
        
        private List<(Vector3, long)> _PositionQueue = new List<(Vector3, long)>();
        
        public UnitController(ulong steamId, UnitView view, IMessageSender messageSender, IServerProvider serverProvider, UnityEventProvider unityEventProvider)
        {
            _SteamId = steamId;
            _View = view;
            _MessageSender = messageSender;
            _ServerProvider = serverProvider;
            _UnityEventProvider = unityEventProvider;
            
            _IsLocal = steamId == SteamUser.GetSteamID().m_SteamID;
            view.gameObject.name = $"Player::STEAMID::{steamId}::{(_IsLocal ? "Local" : "Remote")}";

            _UnityEventProvider.OnFixedUpdate += OnFixedUpdate;
            _UnityEventProvider.OnUpdate += OnUpdate;
        }
        
        public void SetPosition(Vector3 targetPosition, long serverTimestamp)
        {
            _PositionQueue.Add((targetPosition, serverTimestamp));
        }
        
        private void OnUpdate()
        {
            if (_IsLocal)
            {
                // Получаем ввод с клавиатуры
                float horizontal = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
                float vertical = Input.GetAxisRaw("Vertical");     // W/S, ↑/↓

                // Создаем вектор движения (только по X и Z)
                Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

                // Перемещаем трансформ
                _View.transform.position += direction * moveSpeed * Time.deltaTime;
            }
            else
            {
                uint offset = 200;
                var simulationTimestamp = _ServerProvider.CurrentTimestamp - offset;
                if (_PositionQueue.Count < 2)
                    return;
                var startIndex = 0;
                var endIndex = 1;
                for (int i = 0; i < _PositionQueue.Count - 1; i++)
                {
                    if (simulationTimestamp > _PositionQueue[i + 1].Item2)
                    {
                        _PositionQueue.RemoveAt(i);
                        i--;
                    }
                }
            
                if (_PositionQueue.Count < 2)
                    return;
                var start = _PositionQueue[startIndex];
                var end = _PositionQueue[endIndex];
                long previousTimestamp = start.Item2;
                long targetTimestamp = end.Item2;

                float frac = (float)(simulationTimestamp - previousTimestamp) / (float)(targetTimestamp - previousTimestamp);
                _View.transform.position = math.lerp(start.Item1, end.Item1, math.saturate(frac));
            }
        }

        private void OnFixedUpdate()
        {
            if (_IsLocal)
            {
                _MessageSender.SendMessage(new PositionMessage()
                {
                    X = _View.transform.position.x,
                    Y = _View.transform.position.y,
                    Z = _View.transform.position.z,
                    SteamId = SteamUser.GetSteamID().m_SteamID,
                });
            }
        }

        public void Dispose()
        {
            _UnityEventProvider.OnFixedUpdate -= OnFixedUpdate;
            _UnityEventProvider.OnUpdate -= OnUpdate;
        }
    }
}