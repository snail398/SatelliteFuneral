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
        public float mouseSensitivity = 2f;

        private Transform cam;
        private float cameraPitch = 0f;
        
        private readonly List<(float3 position, long serverTimestamp, long receivedTimestamp)> _PositionQueue = new List<(float3, long, long)>();
        private readonly List<(quaternion rotation, long serverTimestamp, long receivedTimestamp)> _RotationQueue = new List<(quaternion, long, long)>();
        
        public UnitController(ulong steamId, UnitView view, IMessageSender messageSender, IServerProvider serverProvider, UnityEventProvider unityEventProvider)
        {
            _SteamId = steamId;
            _View = view;
            _MessageSender = messageSender;
            _ServerProvider = serverProvider;
            _UnityEventProvider = unityEventProvider;
            
            _View.Rigidbody.freezeRotation = true;
            _IsLocal = steamId == SteamUser.GetSteamID().m_SteamID;
            view.gameObject.name = $"Player::STEAMID::{steamId}::{(_IsLocal ? "Local" : "Remote")}";
            view.ChangeView(_IsLocal);
            _UnityEventProvider.OnFixedUpdate += OnFixedUpdate;
            _UnityEventProvider.OnUpdate += OnUpdate;
        }
        
        public void SetPosition(float3 targetPosition, quaternion rotation, long serverTimestamp)
        {
            var simulationTimestamp = _ServerProvider.CurrentTimestamp;
            _PositionQueue.Add((targetPosition, serverTimestamp, simulationTimestamp));
            _RotationQueue.Add((rotation, serverTimestamp, simulationTimestamp));
        }
        
        bool grounded;
        void Jump()
        {
            _View.Rigidbody.linearVelocity = new Vector3(_View.Rigidbody.linearVelocity.x, 0f, _View.Rigidbody.linearVelocity.z);
            _View.Rigidbody.AddForce(Vector3.up * _View.JumpForce, ForceMode.Impulse);
        }
        
        private void OnUpdate()
        {
            if (Input.GetMouseButton(2))
            {
                Debug.Break();
            }
            if (_IsLocal)
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                _View.RotationRoot.Rotate(Vector3.up * mouseX);

                cameraPitch -= mouseY;
                cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
                _View.CameraGo.localEulerAngles = Vector3.right * cameraPitch;
                
                grounded = Physics.Raycast(_View.transform.position, Vector3.down, 0.1f, LayerMask.GetMask("Ground"));

                if (grounded && Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                }
            }
            else
            {
                {
                    if (_PositionQueue.Count < 2)
                        return;
                    if (_PositionQueue.Count > 2)
                    {
                        _PositionQueue.RemoveAt(0);
                    }
                    var startIndex = 0;
                    var endIndex = 1;
                    var start = _PositionQueue[startIndex];
                    var end = _PositionQueue[endIndex];
                    long previousTimestamp = start.serverTimestamp;
                    long targetTimestamp = end.serverTimestamp;
                    var simulationTimestamp = _ServerProvider.CurrentTimestamp + previousTimestamp - end.receivedTimestamp ;

                    float frac = (float)(simulationTimestamp - previousTimestamp) /
                                 (float)(targetTimestamp - previousTimestamp);
                    _View.transform.position = math.lerp(start.position, end.position, math.saturate(frac)); 
                }
                {
                    if (_RotationQueue.Count < 2)
                        return;
                    if (_RotationQueue.Count > 2)
                    {
                        _RotationQueue.RemoveAt(0);
                    }
                    var startIndex = 0;
                    var endIndex = 1;
                    var start = _RotationQueue[startIndex];
                    var end = _RotationQueue[endIndex];
                    long previousTimestamp = start.serverTimestamp;
                    long targetTimestamp = end.serverTimestamp;
                    var simulationTimestamp = _ServerProvider.CurrentTimestamp + previousTimestamp - end.receivedTimestamp ;

                    float frac = (float)(simulationTimestamp - previousTimestamp) / (float)(targetTimestamp - previousTimestamp);
                    _View.RotationRoot.rotation = math.slerp(start.rotation, end.rotation, math.saturate(frac));
                }
            }
        }

        private void OnFixedUpdate()
        {
            if (_IsLocal)
            {
                
                float horizontal = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
                float vertical = Input.GetAxisRaw("Vertical");     // W/S, ↑/↓

                Vector3 moveDirection = _View.RotationRoot.right * horizontal + _View.RotationRoot.forward * vertical;
                Vector3 targetHorizontalVel = moveDirection * _View.MoveSpeed;
                Vector3 currentHorizontalVel = new Vector3(_View.Rigidbody.linearVelocity.x, 0f, _View.Rigidbody.linearVelocity.z);
                Vector3 newHorizontalVel = Vector3.Lerp(
                    currentHorizontalVel,
                    targetHorizontalVel,
                    _View.Acceleration * Time.fixedDeltaTime
                );

                _View.Rigidbody.linearVelocity = new Vector3(
                    newHorizontalVel.x,
                    _View.Rigidbody.linearVelocity.y,
                    newHorizontalVel.z
                );
                
                
                
                
                _MessageSender.SendMessage(new PositionMessage()
                {
                    SteamId = SteamUser.GetSteamID().m_SteamID,
                    Position = _View.transform.position,
                    Rotation = (quaternion)_View.RotationRoot.rotation,
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
            itemView.transform.rotation = quaternion.identity;
        }
        
        public void DropItem(ItemView itemView)
        {
            itemView.transform.SetParent(null);
        }
        
    }
}