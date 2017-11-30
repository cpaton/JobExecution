using System.Diagnostics;
using System.Threading.Tasks;

namespace Executor.Console.Job
{
    public class LaunchDebuggerJob : VoidJob
    {
        protected override Task Execute()
        {
            Debugger.Launch();
            return Task.CompletedTask;
        }
    }
}