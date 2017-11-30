using System;
using System.Threading.Tasks;
using Executor.Console.Commands;

namespace Executor.Console.Executors
{
    public class CommandExecutionRequest<TArgs, TResult> : CommandExecution
    {
        private readonly TaskCompletionSource<TResult> _taskCompletionSource;
        public ICommand<TArgs, TResult> Command { get; }
        public TArgs Args { get; }
        public Task<TResult> ResultTask { get; }

        public CommandExecutionRequest(ICommand<TArgs, TResult> command, TArgs args, string requestTrace) : base(requestTrace)
        {
            Command = command;
            Args = args;
            _taskCompletionSource = new TaskCompletionSource<TResult>();
            ResultTask = _taskCompletionSource.Task;
        }

        public override string ToString()
        {
            return $"[{ExeuctionId}] {Command.GetType().Name}({Args}) - {ResultTask.Status}";
        }

        protected override async Task Execute()
        {
            try
            {
                var commandResult = await Command.Execute(Args);
                _taskCompletionSource.SetResult(commandResult);
            }
            catch (Exception e)
            {
                _taskCompletionSource.SetException(e);
            }
        }
    }
}