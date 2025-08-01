using Client;
using Network.Transport;
using Steamworks;
using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    private IMessageSender _MessageSender;

    public void Setup(IMessageSender messageSender, bool isLocal)
    {
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
    }
}
