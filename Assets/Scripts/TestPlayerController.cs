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

    public void SetPosition(Vector3 targetPosition, uint receivedServerTick)
    {
        _PreviousTargetPosition = _TargetPosition;
        _PreviousReceivedServerTick = _ReceivedServerTick;
        
        _TargetPosition = targetPosition;
        _ReceivedServerTick = receivedServerTick;
    }
    
    public void Setup(IMessageSender messageSender, bool isLocal, IServerProvider serverProvider)
    {
        _ServerProvider = serverProvider;
        _MessageSender = messageSender;
        _IsLocal = isLocal;
    }
    
    public float moveSpeed = 5f;
    private bool _IsLocal;

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
            _MessageSender.SendMessage(new PositionMessage()
            {
                X = transform.position.x,
                Y = transform.position.y,
                Z = transform.position.z,
                SteamId = SteamUser.GetSteamID().m_SteamID,
            });  
        }
        else
        {
            if (_PreviousTargetPosition.Equals(Vector3.zero))
            {
                transform.position = _TargetPosition;
            }
            else
            {
                uint previousTimestamp = _PreviousReceivedServerTick * 50;
                uint targetTimestamp = _ReceivedServerTick * 50;
                var delta = targetTimestamp - previousTimestamp;
                var simulationTimestamp = _ServerProvider.CurrentTimestamp - delta;

                float frac = (simulationTimestamp - previousTimestamp) / (targetTimestamp - previousTimestamp);
                transform.position = math.lerp(_PreviousTargetPosition, _TargetPosition, frac);
            }
        }
    }
}
