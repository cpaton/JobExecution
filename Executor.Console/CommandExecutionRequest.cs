using System;
using System.Threading.Tasks;

namespace Executor.Console
{
    public class CommandExecutionRequest<TArgs, TResult> : CommandExecution
    {
        private readonly TaskCompletionSource<TResult> _taskCompletionSource;
        public ICommand<TArgs, TResult> Command { get; }
        public TArgs Args { get; }
        public Task<TResult> ResultTask { get; }
        public string RequestTrace { get; }
        public TimeSpan RunTime => CompletedTimeUtc - StartTimeUtc;
        public DateTimeOffset StartTimeUtc { get; private set; }
        public DateTimeOffset CompletedTimeUtc { get; private set; }

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

        public override async Task Execute()
        {
            StartTimeUtc = DateTimeOffset.UtcNow;
            try
            {
                var commandResult = await Command.Execute(Args);
                _taskCompletionSource.SetResult(commandResult);
            }
            catch (Exception e)
            {
                _taskCompletionSource.SetException(e);
            }
            finally
            {
                CompletedTimeUtc = DateTimeOffset.UtcNow;
            }
        }
    }
}