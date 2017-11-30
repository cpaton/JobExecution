using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Executor.Console.Job;
using Executor.Console.Util;

namespace Executor.Console.Executors
{
    public class ThreadPoolExecutor : IJobExecutor
    {
        private bool _running = false;
        private readonly List<Task> _outstandingTasks = new List<Task>();
        private readonly object _lockObject = new object();
        private TaskFactory _threadPoolTaskFactory;

        public ThreadPoolExecutor()
        {
            _threadPoolTaskFactory = new TaskFactory(TaskScheduler.Default);
        }

        public Task<TResult> SubmitJob<TResult>(Job<TResult> job)
        {
            if (!_running)
            {
                Logger.Log("No enqueing jobExecution as executor is stopped");
                var taskCompletionSource = new TaskCompletionSource<TResult>();
                taskCompletionSource.SetCanceled();
                return taskCompletionSource.Task;
            }

            var commandExecutionRequest = new JobExecutionRequest<TResult>(job, Environment.StackTrace);
            ExecuteRequest(commandExecutionRequest);
            return commandExecutionRequest.ResultTask;
        }

        public Task SubmitJob(Job<Unit> job)
        {
            return SubmitJob<Unit>(job);
        }

        public void Start()
        {
            _running = true;
            Logger.Log($"[{GetType().Name}] Starting");
        }

        private void ExecuteRequest(JobExecution jobExecution)
        {
            _threadPoolTaskFactory.StartNew(() =>
            {
                Logger.Log($"Executing {jobExecution}");
                var task = jobExecution.ExecuteJob();
                lock (_lockObject)
                {
                    _outstandingTasks.Add(task);
                }
                task.ContinueWith((t, __) =>
                                  {
                                      Logger.Log($"Finished executing {jobExecution}");
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
                                  },
                                  null,
                                  _threadPoolTaskFactory.Scheduler);
            });
        }

        public async Task Stop()
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
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }

            Logger.Log($"[{GetType().Name}] Stopped");
        }
    }
}