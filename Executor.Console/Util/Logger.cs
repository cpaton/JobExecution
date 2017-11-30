using System.Threading;

namespace Executor.Console.Util
{
    public static class Logger
    {
        private static readonly object LockObject = new object();

        public static void Log(string message)
        {
            lock (LockObject)
            {
                var threadIdentiifer = string.IsNullOrWhiteSpace(Thread.CurrentThread.Name) ?
                Thread.CurrentThread.ManagedThreadId.ToString() : Thread.CurrentThread.Name;
                System.Console.WriteLine($"[{threadIdentiifer}] {message}");
            }
        }
    }
}