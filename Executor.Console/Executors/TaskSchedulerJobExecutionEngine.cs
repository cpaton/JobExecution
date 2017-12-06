using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Executor.Console.Executors
{
    public abstract class TaskSchedulerJobExecutionEngine : ExecutionEngine, IJobExecutionEngine
    {
        private readonly List<Task> _outstandingTasks = new List<Task>();
        private int _outstandingCount;
        private readonly TaskFactory _jobTaskFactory;
        private bool _running;
        private readonly TaskFactory _threadPoolTaskFactory;

        protected TaskSchedulerJobExecutionEngine(TaskScheduler taskScheduler)
        {
            _jobTaskFactory = new TaskFactory(taskScheduler);
            _threadPoolTaskFactory = new TaskFactory(TaskScheduler.Default);
        }

        public bool ExecuteJob(JobExecutionRequest jobExecutionRequest)
        {
            if (!_running)
            {
                Log($"Not enqueing {jobExecutionRequest.JobSummary()} as executionEngine is stopped");
                return false;
            }

            ExecuteRequest(jobExecutionRequest);
            return true;
        }

        public void Start()
        {
            _running = true;
            Log("Starting");
        }

        readonly AsyncLocal<JobExecutionRequest> _topLevelJob = new AsyncLocal<JobExecutionRequest>();

        private void ExecuteRequest(JobExecutionRequest jobExecutionRequest)
        {
            if (_topLevelJob.Value != null)
            {
                Log($"Executing {jobExecutionRequest.JobSummary()} directly as called within context of existing job");
                jobExecutionRequest.ExecuteJob().Wait();
            }
            else
            {
                Log($"Enqueing {jobExecutionRequest.JobSummary()}");
                _jobTaskFactory.StartNew(() => DoExecute(jobExecutionRequest));
            }

        }

        private void DoExecute(JobExecutionRequest jobExecutionRequest)
        {
            _topLevelJob.Value = jobExecutionRequest;

            Log($"Executing {jobExecutionRequest.JobSummary()}");
            Interlocked.Increment(ref _outstandingCount);
            var task = jobExecutionRequest.ExecuteJob();
            WithLock(() => _outstandingTasks.Add(task),
                     () => Log($"Failed to add job {jobExecutionRequest.JobSummary()} to running list"));
            try
            {
                task.Wait();
            }
            finally
            {
                Log($"Finished executing {jobExecutionRequest.JobSummary()}");
                WithLock(lockObject =>
                         {
                             _outstandingTasks.Remove(task);
                             Interlocked.Decrement(ref _outstandingCount);
                             Monitor.Pulse(lockObject);
                         },
                         () => Log($"Failed to remove job {jobExecutionRequest.JobSummary()} from running list"));
                if (ReferenceEquals(_topLevelJob.Value, jobExecutionRequest))
                {
                    _topLevelJob.Value = null;
                }
            }
        }

        public async Task Stop(bool waitForCurrentExecutingJobsToComplete = true)
        {
            Log("Stopping");
            _running = false;

            if (!waitForCurrentExecutingJobsToComplete)
            {
                Log("Not waiting for running jobs to complete");
                return;
            }

            await _threadPoolTaskFactory.StartNew(() =>
            {
                var continueToWait = true;
                while (continueToWait)
                {
                    WithLock(lockObject =>
                             {
                                 if (_outstandingCount == 0)
                                 {
                                     Log("All outstanding tasks complete");
                                     continueToWait = false;
                                     return;
                                 }
                                 Log($"Waiting for {_outstandingTasks.Count:#,0} running jobs to complete...");
                                 Monitor.Wait(lockObject);
                             },
                             () => Log("Failed to wait for running jobs to complete"));
                }
            });
            Log("Stopped");
        }
    }
}