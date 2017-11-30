using System;
using System.Threading.Tasks;

namespace Executor.Console
{
    public abstract class CommandExecution
    {
        public abstract Task Execute();
        public Guid ExeuctionId { get; }

        protected CommandExecution(string requestTrace)
        {
            ExeuctionId = Guid.NewGuid();
        }
    }
}