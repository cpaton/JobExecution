using System.Threading.Tasks;

namespace Executor.Console.Job
{
    public abstract class VoidJob : Job<Unit>
    {
        async Task<Unit> Job<Unit>.Execute()
        {
            await Execute();
            return default(Unit);
        }

        protected abstract Task Execute();
    }
}