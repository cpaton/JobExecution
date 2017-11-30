using System;
using System.Threading.Tasks;

namespace Executor.Console
{
    public struct Unit
    {
    }

    public interface ICommand<TArgs, TResult>
    {
        Task<TResult> Execute(TArgs args);
    }
}