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
        public static async Task Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            var executor = new SingleAtOnceExecutor();
            var threadPoolExecutor = new ThreadPoolExecutor();
            executor.Start();
            threadPoolExecutor.Start();

            for (int i = 1; i <= 5; i++)
            {
#pragma warning disable 4014
                executor.SubmitCommandForExecution(new ShortDelayJob($"{i}-single"));
                threadPoolExecutor.SubmitCommandForExecution(new ShortDelayJob($"{i}-pool"));
#pragma warning restore 4014
            }

            await executor.Stop();
            await threadPoolExecutor.Stop();
            Logger.Log("End");
        }
    }
}