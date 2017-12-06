using System;
using System.Threading.Tasks;
using Executor.Console.Util;

namespace Executor.Console.Job
{
    public class ChildJob : VoidJob
    {
        private readonly string _name;

        public ChildJob(string name)
        {
            _name = name;
        }

        protected override async Task Execute()
        {
            Logger.Log($"{this} Starting");
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            Logger.Log($"{this} Ending");
        }

        public override string ToString()
        {
            return $"Child({_name})";
        }
    }
}