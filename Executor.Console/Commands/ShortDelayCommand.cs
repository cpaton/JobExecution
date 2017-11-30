using System;
using System.Threading.Tasks;
using Executor.Console.Util;

namespace Executor.Console.Commands
{
    public class ShortDelayCommand : VoidCommand<string>
    {
        protected override async Task Execute(string name)
        {
            Logger.Log($"[{name}] Starting execution");
            await Task.Delay(TimeSpan.FromSeconds(1));
            Logger.Log($"[{name}] Finishing execution");
        }
    }
}