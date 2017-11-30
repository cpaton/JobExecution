using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Executor.Console
{
    public class SingleCommandAtOnceExecutor
    {
        private readonly Queue<CommandExecution> _executionQueue = new Queue<CommandExecution>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly object _lockObject = new object();
        private readonly ManualResetEvent _stopped = new ManualResetEvent(true);
        private Thread _executionThread;

        public Task<TResult> SubmitCommandForExecution<TArgs, TResult>(ICommand<TArgs, TResult> command, TArgs args)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                Logger.Log("No enqueing commandExecution as executor is stopped");
                var taskCompletionSource = new TaskCompletionSource<TResult>();
                taskCompletionSource.SetCanceled();
                return taskCompletionSource.Task;
            }

            var commandExecutionRequest = new CommandExecutionRequest<TArgs, TResult>(command, args, Environment.StackTrace);

            if (Monitor.TryEnter(_lockObject, TimeSpan.FromMinutes(1)))
            {
                try
                {
                    Logger.Log("Enqueuing command");
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
            Logger.Log("SingleCommandAtOnceExecutor starting");

            _stopped.Reset();
            var cancellationToken = _cancellationTokenSource.Token;

            var continueToProcessWork = true;
            while (continueToProcessWork)
            {
                CommandExecution commandExecution = null;
                if (Monitor.TryEnter(_lockObject, TimeSpan.FromMinutes(1)))
                {
                    try
                    {
                        if (_executionQueue.Count > 0)
                        {
                            commandExecution = _executionQueue.Dequeue();
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

                if (commandExecution != null)
                {
                    ExecuteRequest(commandExecution);
                }
            }

            Logger.Log("Execution ending");
            _stopped.Set();
        }

        private void ExecuteRequest(CommandExecution commandExecution)
        {
            Logger.Log($"Executing {commandExecution}");
            try
            {
                var executionTask = commandExecution.Execute();
                executionTask.Wait();
            }
            catch (Exception e)
            {
                Logger.Log($"Command execution {commandExecution} failed - {e.Message}");
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
                Logger.Log("SingleCommandAtOnceExecutor stopped");
            });
            waitToStopTask.Start();
            return waitToStopTask;
        }
    }
}