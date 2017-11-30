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
            Logger.Log($"[{_name}] Starting execution");
            await Task.Delay(TimeSpan.FromSeconds(1));
            Logger.Log($"[{_name}] Finishing execution");
        }

        public override string ToString()
        {
            return $"{GetType().Name}({_name})";
        }
    }
}