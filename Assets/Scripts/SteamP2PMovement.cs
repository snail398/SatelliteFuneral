using UnityEngine;
using Steamworks;
using System.IO;

public class SteamP2PMovement : MonoBehaviour
{
    public GameObject localCube;
    public GameObject remoteCube;

    private bool isHost;
    private CSteamID remoteId;
    private const byte CHANNEL = 0;

    void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam not initialized!");
            return;
        }

        isHost = SteamUser.GetSteamID().m_SteamID > 76561197960287930; // простая логика для хоста
        Debug.Log("IsHost: " + isHost);

        // прослушка входящих сообщений
        InvokeRepeating(nameof(ReceiveData), 0f, 0.01f);
    }

    void Update()
    {
        HandleInput();
        SendPosition();
    }

    void HandleInput()
    {
        if (localCube == null) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(h, v, 0);
        localCube.transform.position += dir * Time.deltaTime * 5f;
    }

    void SendPosition()
    {
        if (remoteId == CSteamID.Nil) return;

        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            Vector3 pos = localCube.transform.position;
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);

            byte[] data = stream.ToArray();
            SteamNetworking.SendP2PPacket(remoteId, data, (uint)data.Length, EP2PSend.k_EP2PSendUnreliable, CHANNEL);
        }
    }

    void ReceiveData()
    {
        while (SteamNetworking.IsP2PPacketAvailable(out uint size, CHANNEL))
        {
            byte[] buffer = new byte[size];
            if (SteamNetworking.ReadP2PPacket(buffer, size, out uint bytesRead, out CSteamID sender, CHANNEL))
            {
                if (remoteId == CSteamID.Nil)
                    remoteId = sender;

                using (MemoryStream stream = new MemoryStream(buffer))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    remoteCube.transform.position = new Vector3(x, y, z);
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        SteamAPI.Shutdown();
    }
}
