using System.Threading.Tasks;

namespace Executor.Console.Job
{
    public interface Job<TResult>
    {
        Task<TResult> Execute();
    }
}