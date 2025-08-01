using System;
using UnityEngine;

namespace Utils
{
    public class UnityEventProvider : MonoBehaviour {
        public event Action OnUpdate = () => { };
        public event Action OnLateUpdate = () => { };
        public event Action OnFixedUpdate = () => { };
    
        private void Update() {
            OnUpdate.Invoke();
        }

        private void LateUpdate() {
            OnLateUpdate.Invoke();
        }

        private void FixedUpdate() {
            OnFixedUpdate.Invoke();
        }
    }
}