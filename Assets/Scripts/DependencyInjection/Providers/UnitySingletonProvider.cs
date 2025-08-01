using UnityEngine;

namespace DependencyInjection
{
    public class UnitySingletonProvider<T> : IObjectProvider<T> where T : Component
    {
        private T _instance;
        private bool _persistent;

        public UnitySingletonProvider(bool persistent)
        {
            _persistent = persistent;
        }

        public T GetObject(Container container) {
            if (_instance != null)
                return _instance;
            var singleton = new GameObject();
            _instance = singleton.AddComponent<T>();
            singleton.name = typeof(T).ToString();
            if (_persistent)
                Object.DontDestroyOnLoad(_instance.gameObject);
            return _instance;
        }
    }
}