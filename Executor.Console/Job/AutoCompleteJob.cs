using System.Threading.Tasks;
using Executor.Console.Util;

namespace Executor.Console.Job
{
    public class AutoCompleteJob : VoidJob
    {
        private readonly string _name;

        public AutoCompleteJob(string name)
        {
            _name = name;
        }

        protected override Task Execute()
        {
            Logger.Log($"[{ToString()}] Executed");
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"{GetType().Name}({_name})";
        }
    }
}