using System.Diagnostics;
using System.Threading.Tasks;

namespace Executor.Console.Commands
{
    public class LaunchDebuggerCommand : VoidCommand
    {
        protected override Task Execute()
        {
            Debugger.Launch();
            return Task.CompletedTask;
        }
    }
}