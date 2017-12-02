using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Executor.Console.Util;

namespace Executor.Console.Executors
{
    /// <summary>
    ///     Exeuction engines which runs a single job at once using a thread that it manages
    ///     Care must be taken if the Jobs running on this engine start and wait for child jobs to run.
    ///     If the child job, or a child job of the child job attempts to run using the execution engine
    ///     it will deadlock
    /// </summary>
    public class SingleAtOnceExecutionEngine : ExecutionEngine, IJobExecutionEngine
    {
        private readonly Queue<JobExecutionRequest> _executionQueue = new Queue<JobExecutionRequest>();
        private readonly ManualResetEvent _stopped = new ManualResetEvent(true);
        private CancellationTokenSource _cancellationTokenSource;
        private Thread _executionThread;

        public SingleAtOnceExecutionEngine()
        {
            // Initially the engine is stopped
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Cancel();
        }

        public bool ExecuteJob(JobExecutionRequest jobExecutionRequest)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                Logger.Log($"Not enqueing {jobExecutionRequest.JobSummary()} as executionEngine is stopped");
                jobExecutionRequest.Cancel();
                return false;
            }

            WithLock(lockObject =>
                     {
                         Logger.Log($"Enqueuing job {jobExecutionRequest.JobSummary()}");
                         _executionQueue.Enqueue(jobExecutionRequest);
                         Monitor.Pulse(lockObject);
                     },
                     () => throw new Exception(
                         $"Failed to enqueue job {jobExecutionRequest.JobSummary()} as lock could not be obtained in time"));
            return true;
        }

        public void Start()
        {
            if (_executionThread != null && _executionThread.ThreadState != ThreadState.Stopped)
            {
                throw new Exception("Cannot start executionEngine as it is already running");
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _stopped.Reset();
            _executionThread = new Thread(ExecutionLoop)
                               {
                                   Name = GetType().Name
                               };
            _executionThread.Start();
        }

        public Task Stop(bool waitForCurrentExecutingJobsToComplete = true)
        {
            Logger.Log("Requesting executionEngine to stop...");
            _cancellationTokenSource.Cancel();
            WithLock(lockObject => Monitor.Pulse(lockObject),
                     () => throw new Exception("Unable to process request to stop the execution engine as the lock could not be acquired"));

            if (!waitForCurrentExecutingJobsToComplete)
            {
                return Task.CompletedTask;
            }

            Logger.Log("Waiting for executionEngine to stop....");
            var waitToStopTask = Task.Run(() =>
            {
                _stopped.WaitOne();
                Logger.Log("SingleAtOnceExecutionEngine stopped");
            });
            return waitToStopTask;
        }

        private void ExecutionLoop()
        {
            Logger.Log("SingleAtOnceExecutionEngine starting");

            _stopped.Reset();
            var cancellationToken = _cancellationTokenSource.Token;

            var continueToProcessWork = true;
            while (continueToProcessWork)
            {
                JobExecutionRequest jobExecutionRequest = null;
                WithLock(lockObject =>
                         {
                             if (_executionQueue.Count > 0)
                             {
                                 jobExecutionRequest = _executionQueue.Dequeue();
                             }
                             else
                             {
                                 if (!cancellationToken.IsCancellationRequested)
                                 {
                                     Monitor.Wait(lockObject);
                                 }
                                 else
                                 {
                                     continueToProcessWork = false;
                                 }
                             }
                         },
                         () => { });

                if (jobExecutionRequest != null)
                {
                    ExecuteRequest(jobExecutionRequest);
                }
            }

            Logger.Log("Execution ending");
            _stopped.Set();
        }

        private void ExecuteRequest(JobExecutionRequest jobExecutionRequest)
        {
            Logger.Log($"Executing {jobExecutionRequest.JobSummary()}");
            try
            {
                var executionTask = jobExecutionRequest.ExecuteJob();
                executionTask.Wait();
            }
            catch (Exception e)
            {
                Logger.Log($"Job execution {jobExecutionRequest.JobSummary()} failed - {e.Message}");
            }
        }
    }
}