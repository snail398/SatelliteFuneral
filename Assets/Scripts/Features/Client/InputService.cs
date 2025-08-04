using System.Collections.Generic;
using DependencyInjection;
using Network.Transport;
using Utils;

namespace Client
{
    public class InputService : IUnloadableService
    {
        private readonly UnityEventProvider _UnityEventProvider;
        private readonly IMessageSender _MessageSender;
        
        private Dictionary<uint, InputData> _Inputs = new Dictionary<uint, InputData>();
        
        public InputService(UnityEventProvider unityEventProvider, IMessageSender messageSender)
        {
            _UnityEventProvider = unityEventProvider;
            _MessageSender = messageSender;

            _UnityEventProvider.OnFixedUpdate += OnFixedUpdate;
        }

        private void OnFixedUpdate()
        {
            
        }

        public void Unload()
        {
            _UnityEventProvider.OnFixedUpdate -= OnFixedUpdate;
        }
    }
}