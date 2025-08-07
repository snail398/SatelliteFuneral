using System;
using System.Collections.Generic;
using Client.Inventory;
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
        public float mouseSensitivity = 2f;

        private Transform cam;
        private float cameraPitch = 0f;
        
        private List<(float3, long)> _PositionQueue = new List<(float3, long)>();
        private List<(quaternion, long)> _RotationQueue = new List<(quaternion, long)>();
        
        public UnitController(ulong steamId, UnitView view, IMessageSender messageSender, IServerProvider serverProvider, UnityEventProvider unityEventProvider)
        {
            _SteamId = steamId;
            _View = view;
            _MessageSender = messageSender;
            _ServerProvider = serverProvider;
            _UnityEventProvider = unityEventProvider;
            
            _IsLocal = steamId == SteamUser.GetSteamID().m_SteamID;
            view.gameObject.name = $"Player::STEAMID::{steamId}::{(_IsLocal ? "Local" : "Remote")}";
            view.ChangeView(_IsLocal);
            _UnityEventProvider.OnFixedUpdate += OnFixedUpdate;
            _UnityEventProvider.OnUpdate += OnUpdate;
        }
        
        public void SetPosition(float3 targetPosition, quaternion rotation, long serverTimestamp)
        {
            _PositionQueue.Add((targetPosition, serverTimestamp));
            _RotationQueue.Add((rotation, serverTimestamp));
        }
        
        private void OnUpdate()
        {
            if (_IsLocal)
            {
                // Получаем ввод с клавиатуры
                float horizontal = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
                float vertical = Input.GetAxisRaw("Vertical");     // W/S, ↑/↓

                // Создаем вектор движения (только по X и Z)
                Vector3 move = _View.transform.right * horizontal + _View.transform.forward * vertical;

                // Перемещаем трансформ
                _View.transform.position += move * moveSpeed * Time.deltaTime;
                
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                _View.transform.Rotate(Vector3.up * mouseX);

                cameraPitch -= mouseY;
                cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
                _View.FirstPersonView.localEulerAngles = Vector3.right * cameraPitch;
                
                // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // RaycastHit hit;
                //
                // if (Physics.Raycast(ray, out hit))
                // {
                //     var point = hit.point;
                //     point.y = _View.transform.position.y;
                //     hit.point = point;
                //     var targetDirection =(hit.point - _View.transform.position);
                //     _View.transform.rotation = quaternion.LookRotation( math.normalize(targetDirection), Vector3.up);
                // }
                
            }
            else
            {
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

                    float frac = (float)(simulationTimestamp - previousTimestamp) /
                                 (float)(targetTimestamp - previousTimestamp);
                    _View.transform.position = math.lerp(start.Item1, end.Item1, math.saturate(frac));
                }
                {
                    uint offset = 200;
                    var simulationTimestamp = _ServerProvider.CurrentTimestamp - offset;
                    if (_RotationQueue.Count < 2)
                        return;
                    var startIndex = 0;
                    var endIndex = 1;
                    for (int i = 0; i < _RotationQueue.Count - 1; i++)
                    {
                        if (simulationTimestamp > _RotationQueue[i + 1].Item2)
                        {
                            _RotationQueue.RemoveAt(i);
                            i--;
                        }
                    }

                    if (_RotationQueue.Count < 2)
                        return;
                    var start = _RotationQueue[startIndex];
                    var end = _RotationQueue[endIndex];
                    long previousTimestamp = start.Item2;
                    long targetTimestamp = end.Item2;

                    float frac = (float)(simulationTimestamp - previousTimestamp) / (float)(targetTimestamp - previousTimestamp);
                    _View.transform.rotation = math.slerp(start.Item1, end.Item1, math.saturate(frac));
                }
            }
        }

        private void OnFixedUpdate()
        {
            if (_IsLocal)
            {
                _MessageSender.SendMessage(new PositionMessage()
                {
                    SteamId = SteamUser.GetSteamID().m_SteamID,
                    Position = _View.transform.position,
                    Rotation = (quaternion)_View.transform.rotation,
                });
            }
        }

        public void Dispose()
        {
            _UnityEventProvider.OnFixedUpdate -= OnFixedUpdate;
            _UnityEventProvider.OnUpdate -= OnUpdate;
        }

        public void TakeItem(ItemView itemView)
        {
            itemView.transform.SetParent(_View.PossessionPoint);
            itemView.transform.localPosition = Vector3.zero;
        }
        
        public void DropItem(ItemView itemView)
        {
            itemView.transform.SetParent(null);
        }
        
    }
}