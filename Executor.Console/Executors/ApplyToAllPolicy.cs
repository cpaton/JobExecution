using Executor.Console.Job;

namespace Executor.Console.Executors
{
    /// <summary>
    /// Policy which applies to all job types.  Typically used as the catch-all policy to make sure
    /// all jobs get routed to an <see cref="IJobExecutionEngine"/>
    /// </summary>
    public class ApplyToAllPolicy : IExecutionPolicy
    {
        private readonly IJobExecutionEngine _executionEngine;

        public ApplyToAllPolicy(IJobExecutionEngine executionEngine)
        {
            _executionEngine = executionEngine;
        }

        public bool AppliesTo<T>(Job<T> job)
        {
            return true;
        }

        public IJobExecutionEngine EngineFor<T>(Job<T> job)
        {
            return _executionEngine;
        }
    }
}