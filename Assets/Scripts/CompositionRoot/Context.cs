using System.Threading.Tasks;

namespace CompositionRoot
{
    public abstract class Context
    {
        public abstract Task LoadContext();
        public abstract Task UnloadContext();
    }
}