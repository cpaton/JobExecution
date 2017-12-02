using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Executor.Console.Util;

namespace Executor.Console.Executors
{
    /// <summary>
    ///     Runs jobs using the thread pool, as a result jobs can and will run in parallel
    /// </summary>
    public class ThreadPoolExecutionEngine : ExecutionEngine, IJobExecutionEngine
    {
        private readonly List<Task> _outstandingTasks = new List<Task>();
        private readonly TaskFactory _threadPoolTaskFactory;
        private bool _running;

        public ThreadPoolExecutionEngine()
        {
            _threadPoolTaskFactory = new TaskFactory(TaskScheduler.Default);
        }

        public bool ExecuteJob(JobExecutionRequest jobExecutionRequest)
        {
            if (!_running)
            {
                Logger.Log($"Not enqueing {jobExecutionRequest.JobSummary()} as executionEngine is stopped");
                return false;
            }

            ExecuteRequest(jobExecutionRequest);
            return true;
        }

        public void Start()
        {
            _running = true;
            Logger.Log($"[{GetType().Name}] Starting");
        }

        private void ExecuteRequest(JobExecutionRequest jobExecutionRequest)
        {
            _threadPoolTaskFactory.StartNew(() =>
            {
                Logger.Log($"Executing {jobExecutionRequest.JobSummary()}");
                var task = jobExecutionRequest.ExecuteJob();
                WithLock(() => _outstandingTasks.Add(task), 
                    () => Logger.Log($"Failed to add job {jobExecutionRequest.JobSummary()} to running list"));
                task.ContinueWith((t, __) =>
                                  {
                                      Logger.Log($"Finished executing {jobExecutionRequest.JobSummary()}");
                                      WithLock(lockObject =>
                                      {
                                          _outstandingTasks.Remove(task);
                                          Monitor.Pulse(lockObject);
                                      }, () => Logger.Log($"Failed to remove job {jobExecutionRequest.JobSummary()} from running list"));
                                  },
                                  null,
                                  _threadPoolTaskFactory.Scheduler);
            });
        }

        public async Task Stop(bool waitForCurrentExecutingJobsToComplete = true)
        {
            Logger.Log($"[{GetType().Name}] Stopping");
            _running = false;

            if (!waitForCurrentExecutingJobsToComplete)
            {
                Logger.Log($"[{GetType().Name}] Not waiting for running jobs to complete");
                return;
            }

            await _threadPoolTaskFactory.StartNew(() =>
            {
                var continueToWait = true;
                while (continueToWait)
                {
                    WithLock(lockObject =>
                             {
                                 if (_outstandingTasks.Count == 0)
                                 {
                                     continueToWait = false;
                                     return;
                                 }
                                 Logger.Log($"[{GetType().Name}] Waiting for {_outstandingTasks.Count:#,0} running jobs to complete...");
                                 Monitor.Wait(lockObject);
                             },
                             () => Logger.Log($"Failed to wait for running jobs to complete"));
                }
            });
            Logger.Log($"[{GetType().Name}] Stopped");
        }
    }
}