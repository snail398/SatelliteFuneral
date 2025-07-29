using UnityEngine;
using Steamworks;
using System.IO;

public class SteamLobbyManager : MonoBehaviour
{
    public GameObject localCube;
    public GameObject remoteCube;

    private const int lobbyMaxPlayers = 2;
    private const byte CHANNEL = 0;

    private bool isHost = false;
    private CSteamID remoteId;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<LobbyMatchList_t> lobbyMatchList;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<P2PSessionRequest_t> p2pSessionRequest;

    void Start()
    {
        if (!SteamAPI.Init())
        {
            Debug.LogError("Steam API Init failed!");
            return;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyMatchList = Callback<LobbyMatchList_t>.Create(OnLobbyList);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        p2pSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);

        InvokeRepeating(nameof(ReceiveData), 0f, 0.02f);
    }

    void Update()
    {
        if (localCube != null)
            HandleInput();

        if (remoteId != CSteamID.Nil)
            SendPosition();

        SteamAPI.RunCallbacks();
    }

    public void HostLobby()
    {
        isHost = true;
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, lobbyMaxPlayers);
    }

    public void JoinLobby()
    {
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(500);
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
        SteamMatchmaking.AddRequestLobbyListStringFilter("lobby_name", "228", ELobbyComparison.k_ELobbyComparisonEqual);
        SteamMatchmaking.RequestLobbyList();
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Failed to create lobby!");
            return;
        }

        var lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyId, "host_id", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(lobbyId, "lobby_name", "228");
        Debug.Log($"Lobby created. lobby id: {lobbyId}");
    }

    private void OnLobbyList(LobbyMatchList_t callback)
    {
        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            string code = SteamMatchmaking.GetLobbyData(lobbyId, "lobby_name");
            string host = SteamMatchmaking.GetLobbyData(lobbyId, "host_id");
            Debug.LogError($"Lobby found. lobby name: {code}, host: {host}");
            if (code == "228")
            {
                SteamMatchmaking.JoinLobby(lobbyId);
                return;
            }
        }

        Debug.LogWarning("Lobby with code " + 228 + " not found.");
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        var lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        string hostIdStr = SteamMatchmaking.GetLobbyData(lobbyID, "host_id");

        Debug.LogError($"hostIdStr lobby id: {hostIdStr}");
        if (SteamUser.GetSteamID().ToString() != hostIdStr)
        {
            remoteId = new CSteamID(ulong.Parse(hostIdStr));
            Debug.Log("Client joined. Host ID: " + remoteId);
        }
        else
        {
            Debug.Log("Host entered lobby.");
        }
    }

    private void OnP2PSessionRequest(P2PSessionRequest_t request)
    {
        SteamNetworking.AcceptP2PSessionWithUser(request.m_steamIDRemote);
        remoteId = request.m_steamIDRemote;
        Debug.Log("Accepted P2P session from: " + remoteId);
    }

    void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(h, v, 0);
        localCube.transform.position += dir * Time.deltaTime * 4f;
    }

    void SendPosition()
    {
        Vector3 pos = localCube.transform.position;

        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);

            byte[] data = ms.ToArray();
            SteamNetworking.SendP2PPacket(remoteId, data, (uint)data.Length, EP2PSend.k_EP2PSendUnreliable, CHANNEL);
        }
    }

    void ReceiveData()
    {
        while (SteamNetworking.IsP2PPacketAvailable(out uint size, CHANNEL))
        {
            byte[] buffer = new byte[size];
            if (SteamNetworking.ReadP2PPacket(buffer, size, out uint readSize, out CSteamID sender, CHANNEL))
            {
                using (var ms = new MemoryStream(buffer))
                using (var reader = new BinaryReader(ms))
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();

                    remoteCube.transform.position = new Vector3(x, y, z);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        SteamAPI.Shutdown();
    }
}
