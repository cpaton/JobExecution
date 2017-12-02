using System.Threading.Tasks;

namespace Executor.Console.Executors
{
    public interface IJobExecutionEngine
    {
        bool ExecuteJob(JobExecutionRequest jobExecutionRequest);
        void Start();
        Task Stop(bool waitForCurrentExecutingJobsToComplete);
    }
}