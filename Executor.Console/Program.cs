using System;
using System.Threading;
using System.Threading.Tasks;
using Executor.Console.Executors;
using Executor.Console.Job;
using Executor.Console.Util;

namespace Executor.Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            var oneAtATimeEngine = new SingleAtOnceExecutionEngine();
            var orderedExecutionEngine = new OrderedExecutionEngine();
            var threadPoolEngine = new ThreadPoolExecutionEngine();
            oneAtATimeEngine.Start();
            threadPoolEngine.Start();
            orderedExecutionEngine.Start();
            

            var jobExecutor = new JobExecutor(threadPoolEngine);
            jobExecutor.AddPolicies(new OneAtATimePolicy<ShortDelayJob>(orderedExecutionEngine));
            jobExecutor.AddPolicies(new OneAtATimePolicy<ParentJob>(orderedExecutionEngine));
            jobExecutor.AddPolicies(new OneAtATimePolicy<ChildJob>(orderedExecutionEngine));

            for (int i = 1; i <= 3; i++)
            {
#pragma warning disable 4014
                jobExecutor.SubmitJob(new ParentJob(jobExecutor, $"Job{i}"));
                //jobExecutor.SubmitJob(new ShortDelayJob($"Job{i}"));
                //jobExecutor.SubmitJob(new RandomDelayJob($"Job{i}"));
                //jobExecutor.SubmitJob(new AutoCompleteJob($"Job{i}"));
#pragma warning restore 4014
            }

            Task.Delay(TimeSpan.FromSeconds(10)).Wait();
            
            var task1 = oneAtATimeEngine.Stop();
            var task2 = threadPoolEngine.Stop();
            var task3 = orderedExecutionEngine.Stop();
            Task.WaitAll(task1, task2, task3);
            Logger.Log("End");
        }
    }
}