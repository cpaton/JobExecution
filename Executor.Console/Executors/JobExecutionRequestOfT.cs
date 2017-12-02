using System;
using System.Threading.Tasks;
using Executor.Console.Job;

namespace Executor.Console.Executors
{
    public class JobExecutionRequest<TResult> : JobExecutionRequest
    {
        private readonly TaskCompletionSource<TResult> _taskCompletionSource;
        public Job<TResult> Job { get; }
        public Task<TResult> ResultTask { get; }
        protected override Task JobResultTask => ResultTask;

        public JobExecutionRequest(Job<TResult> job, string requestTrace) : base(requestTrace)
        {
            Job = job;
            _taskCompletionSource = new TaskCompletionSource<TResult>();
            ResultTask = _taskCompletionSource.Task;
        }

        public override string JobSummary()
        {
            return $"[{ExeuctionId}] {Job}";
        }

        public override void Cancel()
        {
            _taskCompletionSource.SetCanceled();
        }

        protected override async Task Execute()
        {
            try
            {
                var commandResult = await Job.Execute();
                _taskCompletionSource.SetResult(commandResult);
            }
            catch (Exception e)
            {
                _taskCompletionSource.SetException(e);
            }
        }
    }
}