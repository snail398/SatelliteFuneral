using System.Threading.Tasks;
using Features;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CompositionRoot
{
    public class MenuContext : Context
    {
        public override async Task LoadContext()
        {
            Debug.LogError("Load Menu Context");
            await SceneManager.LoadSceneAsync("Menu");
            CompositionRoot.Container.RegisterSingleton<MenuService, MenuService>().ResolveAndLoad();
        }

        public override async Task UnloadContext()
        {
            Debug.LogError("Unload Menu Context");
        }
    }
}