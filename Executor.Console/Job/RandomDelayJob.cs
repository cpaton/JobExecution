using System;
using System.Threading.Tasks;
using Executor.Console.Util;

namespace Executor.Console.Job
{
    public class RandomDelayJob : VoidJob
    {
        private readonly string _name;

        public RandomDelayJob(string name)
        {
            _name = name;
        }

        protected override async Task Execute()
        {
            var waitTimeMilliseconds = new Random().Next(1000);
            Logger.Log($"[{ToString()}] Starting execution ({waitTimeMilliseconds})");
            await Task.Delay(TimeSpan.FromMilliseconds(waitTimeMilliseconds));
            Logger.Log($"[{ToString()}] Finishing execution");
        }

        public override string ToString()
        {
            return $"{GetType().Name}({_name})";
        }
    }
}