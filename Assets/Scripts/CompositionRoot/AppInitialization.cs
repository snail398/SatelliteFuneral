namespace CompositionRoot
{
    public static class AppInitialization
    {
        public static void InitializeApplication()
        {
            AppInitializationProcessor rootAppInitializer = new RootAppInitializer();
            rootAppInitializer.InitializeApplication();
        }
    }
}