using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Executor.Console.Job;

namespace Executor.Console.Executors
{
    public interface IExecutionPolicy
    {
        bool AppliesTo<T>(Job<T> job);
        IJobExecutor ExecutorFor<T>(Job<T> job);
    }

    public interface IJobExecutor
    {
        Task<TResult> SubmitJob<TResult>(Job<TResult> job);
        Task SubmitJob(Job<Unit> job);
    }

    public class OneAtATimePolicy<TJob> : IExecutionPolicy
    {
        private readonly IJobExecutor _executor;

        public OneAtATimePolicy(IJobExecutor executor)
        {
            _executor = executor;
        }

        public bool AppliesTo<T>(Job<T> job)
        {
            return job is TJob;
        }

        public IJobExecutor ExecutorFor<T>(Job<T> job)
        {
            return _executor;
        }
    }

    public class AppliesToAllPolicy : IExecutionPolicy
    {
        private readonly IJobExecutor _executor;

        public AppliesToAllPolicy(IJobExecutor executor)
        {
            _executor = executor;
        }

        public bool AppliesTo<T>(Job<T> job)
        {
            return true;
        }

        public IJobExecutor ExecutorFor<T>(Job<T> job)
        {
            return _executor;
        }
    }

    public class JobExecutor : IJobExecutor
    {
        private readonly List<IExecutionPolicy> _policies = new List<IExecutionPolicy>();
        private readonly IExecutionPolicy _defaultPolicy;

        public JobExecutor(IJobExecutor defaultExecutor)
        {
            _defaultPolicy = new AppliesToAllPolicy(defaultExecutor);
        }

        public void AddPolicies(params IExecutionPolicy[] policies)
        {
            _policies.AddRange(policies);
        }

        public Task<TResult> SubmitJob<TResult>(Job<TResult> job)
        {
            var executionPolicy = _policies.FirstOrDefault(x => x.AppliesTo(job)) ?? _defaultPolicy;
            var applicableExecutor = executionPolicy.ExecutorFor(job);
            return applicableExecutor.SubmitJob(job);
        }

        public Task SubmitJob(Job<Unit> job)
        {
            return SubmitJob<Unit>(job);
        }
    }
}
