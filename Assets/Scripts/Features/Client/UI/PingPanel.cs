using UnityEngine;

namespace Client.UI
{
    public class PingPanel : MonoBehaviour
    {
        [SerializeField] 
        private Transform _PingItemRoot;

        public Transform PingItemRoot => _PingItemRoot;
    }
}