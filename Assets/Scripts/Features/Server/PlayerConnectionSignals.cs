using Steamworks;

namespace Server
{
    public struct PlayerConnectedSignal
    {
        public CSteamID CSteamID;

        public PlayerConnectedSignal(CSteamID cSteamID)
        {
            CSteamID = cSteamID;
        }
    }
    public struct PlayerLeftSignal
    {
        public CSteamID CSteamID;

        public PlayerLeftSignal(CSteamID cSteamID)
        {
            CSteamID = cSteamID;
        }
    }
}