using System;
using System.Threading.Tasks;

namespace Executor.Console.Executors
{
    public abstract class JobExecution
    {
        protected abstract Task Execute();
        public Guid ExeuctionId { get; }
        public string RequestTrace { get; }
        public TimeSpan RunTime => CompletedTimeUtc - StartTimeUtc;
        public DateTimeOffset RequestTimeUtc { get; private set; }
        public DateTimeOffset StartTimeUtc { get; private set; }
        public DateTimeOffset CompletedTimeUtc { get; private set; }
        public bool HasStarted => StartTimeUtc != default(DateTimeOffset);
        public string RunTimeFriendly => PrettyPrint(RunTime);
        public string DelayBeforeStartingFriendly => PrettyPrint(DelayBeforeStarting);

        public TimeSpan DelayBeforeStarting
        {
            get
            {
                if (!HasStarted)
                {
                    return DateTimeOffset.UtcNow - RequestTimeUtc;
                }
                return StartTimeUtc - RequestTimeUtc;
            }
        }

        private static string PrettyPrint(TimeSpan delayBeforeStarting)
        {
            if (delayBeforeStarting <= TimeSpan.Zero)
            {
                return string.Empty;
            }
            return $"{delayBeforeStarting.Hours}h {delayBeforeStarting.Minutes:00}m {delayBeforeStarting.Seconds:00}s {delayBeforeStarting.Milliseconds:000}ms";
        }

        protected JobExecution(string requestTrace)
        {
            ExeuctionId = Guid.NewGuid();
            RequestTrace = requestTrace;
            RequestTimeUtc = DateTimeOffset.UtcNow;
        }

        public async Task ExecuteJob()
        {
            StartTimeUtc = DateTimeOffset.UtcNow;
            try
            {
                await Execute();
            }
            finally
            {
                CompletedTimeUtc = DateTimeOffset.UtcNow;
            }
        }
    }
}