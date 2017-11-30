using System.Threading.Tasks;

namespace Executor.Console.Commands
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

    public abstract class VoidCommand : ICommand<Unit, Unit>
    {
        async Task<Unit> ICommand<Unit, Unit>.Execute(Unit args)
        {
            await Execute();
            return default(Unit);
        }

        protected abstract Task Execute();
    }
}