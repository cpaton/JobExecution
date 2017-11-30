using System;
using System.Threading.Tasks;
using Executor.Console.Job;

namespace Executor.Console.Executors
{
    public class JobExecutionRequest<TArgs, TResult> : JobExecution
    {
        private readonly TaskCompletionSource<TResult> _taskCompletionSource;
        public Job<TArgs, TResult> Job { get; }
        public TArgs Args { get; }
        public Task<TResult> ResultTask { get; }

        public JobExecutionRequest(Job<TArgs, TResult> job, TArgs args, string requestTrace) : base(requestTrace)
        {
            Job = job;
            Args = args;
            _taskCompletionSource = new TaskCompletionSource<TResult>();
            ResultTask = _taskCompletionSource.Task;
        }

        public override string ToString()
        {
            return $"[{ExeuctionId}] {Job.GetType().Name}({Args}) - {ResultTask.Status}";
        }

        protected override async Task Execute()
        {
            try
            {
                var commandResult = await Job.Execute(Args);
                _taskCompletionSource.SetResult(commandResult);
            }
            catch (Exception e)
            {
                _taskCompletionSource.SetException(e);
            }
        }
    }
}