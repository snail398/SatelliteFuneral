using DependencyInjection;
using Network.Transport;
using Steamworks;
using UnityEngine;
using Utils;

namespace Client
{
    public class SpawnService : ILoadableService, IUnloadableService
    {
        private readonly UnityEventProvider _UnityEventProvider;
        private readonly IMessageSender _MessageSender;

        public SpawnService(UnityEventProvider unityEventProvider, IMessageSender messageSender)
        {
            _UnityEventProvider = unityEventProvider;
            _MessageSender = messageSender;
        }

        void ILoadableService.Load()
        {
            _UnityEventProvider.OnUpdate += Update;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _MessageSender.SendMessage(new SpawnMessage()
                {
                    PlayerId = SteamUser.GetSteamID().m_SteamID,
                });
            }
        }

        void IUnloadableService.Unload()
        {
            _UnityEventProvider.OnUpdate -= Update;
        }
    }
}