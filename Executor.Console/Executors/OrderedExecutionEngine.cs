using System.Threading.Tasks.Schedulers;

namespace Executor.Console.Executors
{
    /// <summary>
    /// Uses the <see cref="OrderedTaskScheduler"/> from the Parallel Extensions Extra
    /// library to provide the execution of one task at a time.  This doesn't provide support
    /// for stopping and cancelling tasks that have been queued
    /// </summary>
    public class OrderedExecutionEngine : TaskSchedulerJobExecutionEngine
    {
        public OrderedExecutionEngine() : base(new OrderedTaskScheduler())
        {
        }
    }
}