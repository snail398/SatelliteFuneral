using System;
using System.Collections.Generic;
using Client;
using Network.Transport;
using Steamworks;
using Unity.Mathematics;
using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    private IMessageSender _MessageSender;
    private IServerProvider _ServerProvider;

    
    private Vector3 _PreviousTargetPosition;
    public uint _PreviousReceivedServerTick;
    
    private Vector3 _TargetPosition;
    public uint _ReceivedServerTick;
    
    private List<(Vector3, int)> _PositionQueue = new List<(Vector3, int)>();

    public void SetPosition(Vector3 targetPosition, int serverTimestamp)
    {
        _PositionQueue.Add((targetPosition, serverTimestamp));
        // _PreviousTargetPosition = _TargetPosition;
        // _PreviousReceivedServerTick = _ReceivedServerTick;
        
        // _TargetPosition = targetPosition;
        // _ReceivedServerTick = receivedServerTick;
    }
    
    public void Setup(IMessageSender messageSender, bool isLocal, IServerProvider serverProvider)
    {
        _ServerProvider = serverProvider;
        _MessageSender = messageSender;
        _IsLocal = isLocal;
    }
    
    public float moveSpeed = 5f;
    private bool _IsLocal;

    private void FixedUpdate()
    {
        if (_IsLocal)
        {
            _MessageSender.SendMessage(new PositionMessage()
            {
                X = transform.position.x,
                Y = transform.position.y,
                Z = transform.position.z,
                SteamId = SteamUser.GetSteamID().m_SteamID,
            });
        }
    }

    void Update()
    {
        if (_IsLocal)
        {
            // Получаем ввод с клавиатуры
            float horizontal = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
            float vertical = Input.GetAxisRaw("Vertical");     // W/S, ↑/↓

            // Создаем вектор движения (только по X и Z)
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            // Перемещаем трансформ
            transform.position += direction * moveSpeed * Time.deltaTime;
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
                if (simulationTimestamp > _PositionQueue[i + 1].Item2 * 50)
                {
                    _PositionQueue.RemoveAt(i);
                    i--;
                }
            }
            
            var start = _PositionQueue[startIndex];
            var end = _PositionQueue[endIndex];
            int previousTimestamp = start.Item2;
            int targetTimestamp = end.Item2;

            float frac = (float)(simulationTimestamp - previousTimestamp) / (float)(targetTimestamp - previousTimestamp);
            transform.position = math.lerp(start.Item1, end.Item1, math.saturate(frac));
        }
    }
}
