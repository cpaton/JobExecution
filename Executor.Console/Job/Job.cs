using System.Threading.Tasks;

namespace Executor.Console.Job
{
    public interface Job<TArgs, TResult>
    {
        Task<TResult> Execute(TArgs args);
    }
}