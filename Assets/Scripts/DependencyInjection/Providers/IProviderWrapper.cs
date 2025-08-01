namespace DependencyInjection
{
    public interface IProviderWrapper
    {
        object GetObject(Container container);
    }
}