using System.Threading.Tasks;

namespace Executor.Console.Commands
{
    public interface ICommand<TArgs, TResult>
    {
        Task<TResult> Execute(TArgs args);
    }
}