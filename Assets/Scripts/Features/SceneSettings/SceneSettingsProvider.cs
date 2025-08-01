using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features
{
    [DefaultExecutionOrder(-999)]
    public class SceneSettingsProvider : MonoBehaviour
    {
        public List<GameObject> SceneSettingsObjects;
        private void Awake()
        {
            var settings = GatherSceneSettings();
            foreach (var setting in settings) {
                CompositionRoot.CompositionRoot.Container.RegisterInstance(setting, setting.GetType());
            }
        }

        protected List<ISceneSetting> GatherSceneSettings()
        {
            return SceneSettingsObjects.SelectMany(_ => _.GetComponents<ISceneSetting>()).ToList();
        }
    }
}