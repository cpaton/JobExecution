using Executor.Console.Job;

namespace Executor.Console.Executors
{
    /// <summary>
    /// Represents a decision about which <see cref="IJobExecutionEngine"/> should be used to run a Job
    /// </summary>
    public interface IExecutionPolicy
    {
        /// <summary>
        /// Does this policy apply to a given Job
        /// </summary>
        bool AppliesTo<T>(Job<T> job);

        /// <summary>
        /// Finds the <see cref="IJobExecutionEngine"/> that should be used to execute the given job
        /// </summary>
        IJobExecutionEngine EngineFor<T>(Job<T> job);
    }
}