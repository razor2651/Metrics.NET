using System;
using System.Threading;
using System.Threading.Tasks;

using Metrics.Utils;

namespace Metrics.Tests
{
    /// <summary>
    ///     Utility class for manually executing the scheduled task.
    /// </summary>
    public sealed class TestScheduler : Scheduler
    {
        public TestScheduler(TestClock clock)
        {
            this.clock = clock;
            this.clock.Advanced += (s, l) => RunIfNeeded();
        }

        public void Start(TimeSpan interval, Func<CancellationToken, Task> task)
        {
            Start(interval, (t) => task(t).Wait());
        }

        public void Start(TimeSpan interval, Func<Task> task)
        {
            Start(interval, () => task().Wait());
        }

        public void Start(TimeSpan interval, Action action)
        {
            Start(interval, t => action());
        }

        public void Start(TimeSpan interval, Action<CancellationToken> action)
        {
            if (interval.TotalSeconds == 0)
            {
                throw new ArgumentException("interval must be > 0 seconds", "interval");
            }

            this.interval = interval;
            lastRun = clock.Seconds;
            this.action = action;
        }

        private void RunIfNeeded()
        {
            long clockSeconds = clock.Seconds;
            long elapsed = clockSeconds - lastRun;
            var times = elapsed / interval.TotalSeconds;
            using (CancellationTokenSource ts = new CancellationTokenSource())
                while (times-- >= 1)
                {
                    lastRun = clockSeconds;
                    action(ts.Token);
                }
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }

        private readonly TestClock clock;
        private TimeSpan interval;
        private Action<CancellationToken> action;
        private long lastRun;
    }
}