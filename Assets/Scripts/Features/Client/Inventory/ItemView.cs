using Unity.Mathematics;
using UnityEngine;

namespace Client.Inventory
{
    public class ItemView : MonoBehaviour
    {

        private bool _IsOwner;
        private uint _ItemId;
        public uint ItemId => _ItemId;

        public void Initialize(bool isOwner, uint itemId)
        {
            _IsOwner = isOwner;
            _ItemId = itemId;
        }
        
        public void SetPosition(float3 position, quaternion rotation, long timestamp)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}