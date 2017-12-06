using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Executor.Console.Executors;
using Executor.Console.Util;

namespace Executor.Console.Job
{
    public class ParentJob : VoidJob
    {
        private readonly JobExecutor _executor;
        private readonly string _name;

        public ParentJob(JobExecutor executor, string name)
        {
            _executor = executor;
            _name = name;
        }

        protected override async Task Execute()
        {
            Logger.Log($"{this} Starting");
            var childJob = new ChildJob(_name);
            await _executor.SubmitJob(childJob).ConfigureAwait(false);
            Logger.Log($"{this} Ending");
        }

        public override string ToString()
        {
            return $"ParentJob({_name})";
        }
    }
}
