using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Executor.Console.Job;
using Executor.Console.Util;

namespace Executor.Console.Executors
{
    public class ThreadPoolExecutor
    {
        private bool _running = false;
        private readonly List<Task> _outstandingTasks = new List<Task>();
        private readonly object _lockObject = new object();

        public Task<TResult> SubmitCommandForExecution<TArgs, TResult>(Job<TArgs, TResult> job, TArgs args)
        {
            if (!_running)
            {
                Logger.Log("No enqueing jobExecution as executor is stopped");
                var taskCompletionSource = new TaskCompletionSource<TResult>();
                taskCompletionSource.SetCanceled();
                return taskCompletionSource.Task;
            }

            var commandExecutionRequest = new JobExecutionRequest<TArgs, TResult>(job, args, Environment.StackTrace);
            ExecuteRequest(commandExecutionRequest);
            return commandExecutionRequest.ResultTask;
        }

        internal void SubmitCommandForExecution(VoidJob job)
        {
            SubmitCommandForExecution(job, default(Unit));
        }

        public void Start()
        {
            _running = true;
            Logger.Log($"[{GetType().Name}] Starting");
        }

        private void ExecuteRequest(JobExecution jobExecution)
        {
            Logger.Log($"Executing {jobExecution}");
            ThreadPool.QueueUserWorkItem(_ => 
            {
                var task = jobExecution.ExecuteJob();
                _outstandingTasks.Add(task);
                task.ContinueWith((t, __) => 
                {
                    if (Monitor.TryEnter(_lockObject, TimeSpan.FromMinutes(1)))
                    {
                        try 
                        {
                            _outstandingTasks.Remove(task);
                            Monitor.Pulse(_lockObject);
                        }
                        finally 
                        {
                            Monitor.Exit(_lockObject);
                        }
                    }
                }, null);
            });
        }

        public Task Stop()
        {
            Logger.Log($"[{GetType().Name}] Stopping");
            _running = false;
            
            while (true)
            {
                if (Monitor.TryEnter(_lockObject, TimeSpan.FromMinutes(1)))
                {
                    try 
                    {
                        if (_outstandingTasks.Count == 0)
                        {
                            break;
                        }
                    }
                    finally 
                    {
                        Monitor.Exit(_lockObject);
                    }
                }
            }

            Logger.Log($"[{GetType().Name}] Stopped");
            return Task.CompletedTask;
        }
    }
}