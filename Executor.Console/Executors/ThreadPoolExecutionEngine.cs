using System.Threading.Tasks;

namespace Executor.Console.Executors
{
    /// <summary>
    ///     Runs jobs using the thread pool, as a result jobs can and will run in parallel
    /// </summary>
    public class ThreadPoolExecutionEngine : TaskSchedulerJobExecutionEngine
    {
        public ThreadPoolExecutionEngine() : base(TaskScheduler.Default)
        {
        }
    }
}