using System.Collections.Generic;
using System.Linq;
using Features;
using UnityEngine;

namespace Client.Inventory
{
    public class SpawnItemSettings : MonoBehaviour, ISceneSetting
    {
        public List<ItemSpawner> _ItemSpawners;

        private void OnValidate()
        {
            _ItemSpawners = GetComponentsInChildren<ItemSpawner>().ToList();
        }
    }
}