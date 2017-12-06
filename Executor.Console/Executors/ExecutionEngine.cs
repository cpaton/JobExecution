using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Executor.Console.Util;

namespace Executor.Console.Executors
{
    public abstract class ExecutionEngine 
    {
        protected ExecutionEngine()
        {
            LockObject = new object();
        }

        private object LockObject { get; }

        protected void WithLock(Action toPerformWithinLock, Action ifUnableToObtainLock)
        {
            WithLock(_ => toPerformWithinLock(), ifUnableToObtainLock);
        }

        protected void Log(string message)
        {
            Logger.Log($"[{GetType().Name}] {message}");
        }

        protected void WithLock(Action<object> toPerformWithinLock, Action ifUnableToObtainLock)
        {
            if (Monitor.TryEnter(LockObject, TimeSpan.FromMinutes(1)))
            {
                try
                {
                    toPerformWithinLock(LockObject);
                }
                finally
                {
                    Monitor.Exit(LockObject);
                }
            }
            else
            {
                ifUnableToObtainLock();
            }
        }

    }
}
