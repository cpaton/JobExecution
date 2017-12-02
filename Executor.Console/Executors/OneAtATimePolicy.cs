using Executor.Console.Job;

namespace Executor.Console.Executors
{
    /// <summary>
    /// Policy that will tie a specific type of Job so that only one Job of that type will run at once.
    /// Subequent jobs of that type will be queued until the previous job has completed
    /// </summary>
    public class OneAtATimePolicy<TJob> : IExecutionPolicy
    {
        private readonly IJobExecutionEngine _executionEngine;

        public OneAtATimePolicy(IJobExecutionEngine executionEngine)
        {
            _executionEngine = executionEngine;
        }

        public bool AppliesTo<T>(Job<T> job)
        {
            return job is TJob;
        }

        public IJobExecutionEngine EngineFor<T>(Job<T> job)
        {
            return _executionEngine;
        }
    }
}