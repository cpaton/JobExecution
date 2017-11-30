using System.Threading.Tasks;

namespace Executor.Console.Job
{
    public abstract class VoidJob<TArgs> : Job<TArgs, Unit>
    {
        async Task<Unit> Job<TArgs, Unit>.Execute(TArgs args)
        {
            await Execute(args);
            return default(Unit);
        }

        protected abstract Task Execute(TArgs args);
    }

    public abstract class VoidJob : Job<Unit, Unit>
    {
        async Task<Unit> Job<Unit, Unit>.Execute(Unit args)
        {
            await Execute();
            return default(Unit);
        }

        protected abstract Task Execute();
    }
}