using System;
using System.Threading.Tasks;
using Executor.Console.Util;

namespace Executor.Console.Job
{
    public class ShortDelayJob : VoidJob
    {
        private readonly string _name;

        public ShortDelayJob(string name)
        {
            _name = name;
        }

        protected override async Task Execute()
        {
            Logger.Log($"[{ToString()}] Starting execution");
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            Logger.Log($"[{ToString()}] Finishing execution");
        }

        public override string ToString()
        {
            return $"{GetType().Name}({_name})";
        }
    }
}