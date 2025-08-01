namespace DependencyInjection
{
    public interface IObjectProvider<T> where T : class
    {
        T GetObject(Container container);
    }
}