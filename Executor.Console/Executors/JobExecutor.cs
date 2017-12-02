using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Executor.Console.Job;

namespace Executor.Console.Executors
{
    public class JobExecutor
    {
        private readonly List<IExecutionPolicy> _policies = new List<IExecutionPolicy>();
        private readonly IExecutionPolicy _defaultPolicy;

        public JobExecutor(IJobExecutionEngine defaultExecutionEngine)
        {
            _defaultPolicy = new ApplyToAllPolicy(defaultExecutionEngine);
        }

        public void AddPolicies(params IExecutionPolicy[] policies)
        {
            _policies.AddRange(policies);
        }

        public Task<TResult> SubmitJob<TResult>(Job<TResult> job)
        {
            var jobExecutionRequest = new JobExecutionRequest<TResult>(job, Environment.StackTrace);

            var executionPolicy = _policies.FirstOrDefault(x => x.AppliesTo(job)) ?? _defaultPolicy;
            var applicableExecutor = executionPolicy.EngineFor(job);
            applicableExecutor.ExecuteJob(jobExecutionRequest);

            return jobExecutionRequest.ResultTask;
        }

        public Task SubmitJob(Job<Unit> job)
        {
            return SubmitJob<Unit>(job);
        }
    }
}
