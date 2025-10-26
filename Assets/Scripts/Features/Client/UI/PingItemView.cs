using TMPro;
using UnityEngine;

namespace Client.UI
{
    public class PingItemView : MonoBehaviour
    {
        [SerializeField] 
        private TextMeshProUGUI _Name;
        [SerializeField] 
        private TextMeshProUGUI _Ping;

        public void SetData(string nickname, int ping)
        {
            _Name.text = nickname;
            _Ping.text = ping.ToString();
        }
    }
}