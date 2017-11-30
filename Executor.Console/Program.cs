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
            var executor = new SingleAtOnceExecutor();
            var threadPoolExecutor = new ThreadPoolExecutor();
            executor.Start();
            threadPoolExecutor.Start();


            var jobExecutor = new JobExecutor(threadPoolExecutor);
            var oneAtATimePolicy = new OneAtATimePolicy<ShortDelayJob>(executor);
            jobExecutor.AddPolicies(oneAtATimePolicy);

            for (int i = 1; i <= 2; i++)
            {
#pragma warning disable 4014
                jobExecutor.SubmitJob(new ShortDelayJob($"Job{i}"));
                jobExecutor.SubmitJob(new RandomDelayJob($"Job{i}"));
                jobExecutor.SubmitJob(new AutoCompleteJob($"Job{i}"));
#pragma warning restore 4014
            }

            var task1 = executor.Stop();
            var task2 = threadPoolExecutor.Stop();
            Task.WaitAll(task1, task2);
            Logger.Log("End");
        }
    }
}