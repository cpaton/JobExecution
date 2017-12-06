using System;
using System.Text;
using System.Threading.Tasks;
using Executor.Console.Util;

namespace Executor.Console.Executors
{
    public abstract class JobExecutionEngine : IJobExecutionEngine
    {
        public abstract bool ExecuteJob(JobExecutionRequest jobExecutionRequest);
        public abstract void Start();
        public abstract Task Stop(bool waitForCurrentExecutingJobsToComplete);

        
    }
}
