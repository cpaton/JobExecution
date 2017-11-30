using System.Threading.Tasks;

namespace Executor.Console
{
    public abstract class VoidCommand<TArgs> : ICommand<TArgs, Unit>
    {
        async Task<Unit> ICommand<TArgs, Unit>.Execute(TArgs args)
        {
            await Execute(args);
            return default(Unit);
        }

        protected abstract Task Execute(TArgs args);
    }
}