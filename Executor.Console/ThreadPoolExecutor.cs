using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Executor.Console
{
    public class ThreadPoolExecutor
    {
        private bool _running = false;
        private List<Task> _outstandingTasks = new List<Task>();
        private object _lockObject = new object();

        public Task<TResult> SubmitCommandForExecution<TArgs, TResult>(ICommand<TArgs, TResult> command, TArgs args)
        {
            if (!_running)
            {
                Logger.Log("No enqueing commandExecution as executor is stopped");
                var taskCompletionSource = new TaskCompletionSource<TResult>();
                taskCompletionSource.SetCanceled();
                return taskCompletionSource.Task;
            }

            var commandExecutionRequest = new CommandExecutionRequest<TArgs, TResult>(command, args, Environment.StackTrace);
            ExecuteRequest(commandExecutionRequest);
            return commandExecutionRequest.ResultTask;
        }

        public void Start()
        {
            _running = true;
            Logger.Log($"[{GetType().Name}] Starting");
        }

        private void ExecuteRequest(CommandExecution commandExecution)
        {
            Logger.Log($"Executing {commandExecution}");
            ThreadPool.QueueUserWorkItem(_ => 
            {
                var task = commandExecution.Execute();
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