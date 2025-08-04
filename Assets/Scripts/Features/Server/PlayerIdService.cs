using System.Collections.Generic;
using Utils;

namespace Server
{
    public class PlayerIdService
    {
        private readonly SignalBus _SignalBus;
        
        private readonly Dictionary<ulong, byte> _SteamIdToPlayerId = new Dictionary<ulong, byte>();
        private byte _FreePLayerId;
        
        public PlayerIdService(SignalBus signalBus)
        {
            _SignalBus = signalBus;
            _SignalBus.Subscribe<PlayerConnectedSignal>(OnPlayerConnected, this);
        }

        private void OnPlayerConnected(PlayerConnectedSignal signal)
        {
            if (_SteamIdToPlayerId.ContainsKey(signal.CSteamID.m_SteamID))
                return;
            _SteamIdToPlayerId[signal.CSteamID.m_SteamID] = _FreePLayerId;
            _FreePLayerId++;
        }
    }
}