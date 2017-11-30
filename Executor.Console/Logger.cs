using System.Threading;

namespace Executor.Console
{
    public static class Logger
    {
        private static readonly object LockObject = new object();

        public static void Log(string message)
        {
            lock (LockObject)
            {
                System.Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId} ({Thread.CurrentThread.Name})] {message}");
            }
        }
    }
}