using UnityEngine;
using UnityEngine.UI;

namespace Features
{
    public class MenuSceneSettings : MonoBehaviour, ISceneSetting
    {
        [SerializeField]
        private Button _HostGameButton;
        [SerializeField]
        private Button _JoinGameButton;

        public Button HostGameButton => _HostGameButton;
        public Button JoinGameButton => _JoinGameButton;
    }
}