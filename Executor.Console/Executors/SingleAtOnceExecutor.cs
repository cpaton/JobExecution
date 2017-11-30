using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Executor.Console.Job;
using Executor.Console.Util;

namespace Executor.Console.Executors
{
    public class SingleAtOnceExecutor
    {
        private readonly Queue<JobExecution> _executionQueue = new Queue<JobExecution>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly object _lockObject = new object();
        private readonly ManualResetEvent _stopped = new ManualResetEvent(true);
        private Thread _executionThread;

        public Task<TResult> SubmitCommandForExecution<TResult>(Job<TResult> job)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                Logger.Log("No enqueing jobExecution as executor is stopped");
                var taskCompletionSource = new TaskCompletionSource<TResult>();
                taskCompletionSource.SetCanceled();
                return taskCompletionSource.Task;
            }

            var commandExecutionRequest = new JobExecutionRequest<TResult>(job, Environment.StackTrace);

            if (Monitor.TryEnter(_lockObject, TimeSpan.FromMinutes(1)))
            {
                try
                {
                    Logger.Log("Enqueuing job");
                    _executionQueue.Enqueue(commandExecutionRequest);
                    Monitor.Pulse(_lockObject);
                }
                finally
                {
                    Monitor.Exit(_lockObject);
                }
            }

            return commandExecutionRequest.ResultTask;
        }

        public void Start()
        {
            if (_executionThread != null && _executionThread.ThreadState != ThreadState.Stopped)
            {
                throw new Exception("Cannot start executor as it is already running");
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _executionThread = new Thread(ExecutionLoop)
                               {
                                   Name = GetType().Name
                               };
            _executionThread.Start();
        }

        private void ExecutionLoop()
        {
            Logger.Log("SingleAtOnceExecutor starting");

            _stopped.Reset();
            var cancellationToken = _cancellationTokenSource.Token;

            var continueToProcessWork = true;
            while (continueToProcessWork)
            {
                JobExecution jobExecution = null;
                if (Monitor.TryEnter(_lockObject, TimeSpan.FromMinutes(1)))
                {
                    try
                    {
                        if (_executionQueue.Count > 0)
                        {
                            jobExecution = _executionQueue.Dequeue();
                        }
                        else
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                Monitor.Wait(_lockObject);
                            }
                            else
                            {
                                continueToProcessWork = false;
                            }
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_lockObject);
                    }
                }

                if (jobExecution != null)
                {
                    ExecuteRequest(jobExecution);
                }
            }

            Logger.Log("Execution ending");
            _stopped.Set();
        }

        private void ExecuteRequest(JobExecution jobExecution)
        {
            Logger.Log($"Executing {jobExecution}");
            try
            {
                var executionTask = jobExecution.ExecuteJob();
                executionTask.Wait();
            }
            catch (Exception e)
            {
                Logger.Log($"Job execution {jobExecution} failed - {e.Message}");
            }
        }

        public Task Stop()
        {
            Logger.Log("Requesting executor to stop...");
            _cancellationTokenSource.Cancel();
            if (Monitor.TryEnter(_lockObject, TimeSpan.FromMinutes(1)))
            {
                try
                {
                    Monitor.Pulse(_lockObject);
                }
                finally
                {
                    Monitor.Exit(_lockObject);
                }
            }

            Logger.Log("Waiting for executor to stop....");
            var waitToStopTask = new Task(() =>
            {
                _stopped.WaitOne();
                Logger.Log("SingleAtOnceExecutor stopped");
            });
            waitToStopTask.Start();
            return waitToStopTask;
        }
    }
}