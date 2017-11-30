using System;
using System.Threading.Tasks;

namespace Executor.Console.Executors
{
    public abstract class CommandExecution
    {
        public abstract Task Execute();
        public Guid ExeuctionId { get; }
        public string RequestTrace { get; }

        protected CommandExecution(string requestTrace)
        {
            ExeuctionId = Guid.NewGuid();
            RequestTrace = requestTrace;
        }
    }
}