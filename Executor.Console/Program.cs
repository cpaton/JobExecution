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
            var threadPoolEngine = new ThreadPoolExecutionEngine();
            oneAtATimeEngine.Start();
            threadPoolEngine.Start();


            var jobExecutor = new JobExecutor(threadPoolEngine);
            var oneAtATimePolicy = new OneAtATimePolicy<ShortDelayJob>(oneAtATimeEngine);
            jobExecutor.AddPolicies(oneAtATimePolicy);

            for (int i = 1; i <= 5; i++)
            {
#pragma warning disable 4014
                jobExecutor.SubmitJob(new ShortDelayJob($"Job{i}"));
                jobExecutor.SubmitJob(new RandomDelayJob($"Job{i}"));
                jobExecutor.SubmitJob(new AutoCompleteJob($"Job{i}"));
#pragma warning restore 4014
            }

            var task1 = oneAtATimeEngine.Stop();
            var task2 = threadPoolEngine.Stop();
            Task.WaitAll(task1, task2);
            Logger.Log("End");
        }
    }
}