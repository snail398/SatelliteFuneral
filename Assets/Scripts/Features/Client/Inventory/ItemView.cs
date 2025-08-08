using Unity.Mathematics;
using UnityEngine;

namespace Client.Inventory
{
    public class ItemView : MonoBehaviour
    {
        [SerializeField]
        private Outline _Outline;
        
        private bool _IsOwner;
        private uint _ItemId;
        public uint ItemId => _ItemId;

        private void Awake()
        {
            _Outline.enabled = false;
        }

        public void Initialize(bool isOwner, uint itemId)
        {
            _IsOwner = isOwner;
            _ItemId = itemId;
        }

        public void SetInteractable(bool isInteractable)
        {
            _Outline.enabled = isInteractable;
        }
        
        public void SetPosition(float3 position, quaternion rotation, long timestamp)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}