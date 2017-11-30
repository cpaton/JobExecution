using System;
using System.Threading;
using System.Threading.Tasks;

namespace Executor.Console
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            var executor = new SingleCommandAtOnceExecutor();
            var threadPoolExecutor = new ThreadPoolExecutor();
            executor.Start();
            threadPoolExecutor.Start();

            var shortDelayCommand = new ShortDelayCommand();
            for (int i = 1; i <= 1; i++)
            {
#pragma warning disable 4014
                executor.SubmitCommandForExecution(shortDelayCommand, $"{i}-single");
                threadPoolExecutor.SubmitCommandForExecution(shortDelayCommand, $"{i}-pool");
#pragma warning restore 4014
            }

            await executor.Stop();
            await threadPoolExecutor.Stop();
            Logger.Log("End");
        }
    }
}