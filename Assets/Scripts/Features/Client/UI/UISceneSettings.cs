using Features;
using UnityEngine;

namespace Client.UI
{
    public class UISceneSettings : MonoBehaviour, ISceneSetting
    {
        [SerializeField]
        private PingPanel _PingPanel;

        public PingPanel PingPanel => _PingPanel;
    }
}