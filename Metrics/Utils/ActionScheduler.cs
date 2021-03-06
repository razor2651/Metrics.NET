using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.Utils
{
    /// <summary>
    ///     Utility class to schedule an Action to be executed repeatedly according to the interval.
    /// </summary>
    /// <remarks>
    ///     The scheduling code is inspired form Daniel Crenna's metrics port
    ///     https://github.com/danielcrenna/metrics-net/blob/master/src/metrics/Reporting/ReporterBase.cs
    /// </remarks>
    public sealed class ActionScheduler : Scheduler
    {
        public ActionScheduler(int toleratedConsecutiveFailures = 0)
        {
            if (toleratedConsecutiveFailures < -1)
            {
                throw new ArgumentException($"{nameof(toleratedConsecutiveFailures)} must be >= -1");
            }
            this.toleratedConsecutiveFailures = toleratedConsecutiveFailures;
        }

        public void Start(TimeSpan interval, Action action)
        {
            Start(interval, t =>
                {
                    if (!t.IsCancellationRequested)
                    {
                        action();
                    }
                });
        }

        public void Start(TimeSpan interval, Action<CancellationToken> action)
        {
            Start(interval, t =>
                {
                    action(t);
                    return Task.FromResult(true);
                });
        }

        public void Start(TimeSpan interval, Func<Task> action)
        {
            Start(interval, t => t.IsCancellationRequested ? action() : Task.FromResult(true));
        }

        public void Start(TimeSpan interval, Func<CancellationToken, Task> action)
        {
            if (interval.TotalSeconds == 0)
            {
                throw new ArgumentException("interval must be > 0 seconds", nameof(interval));
            }

            if (token != null)
            {
                throw new InvalidOperationException("Scheduler is already started.");
            }

            token = new CancellationTokenSource();

            RunScheduler(interval, action, token, toleratedConsecutiveFailures);
        }

        private static void RunScheduler(TimeSpan interval, Func<CancellationToken, Task> action, CancellationTokenSource token, int toleratedConsecutiveFailures)
        {
            Task.Factory.StartNew(async () =>
                {
                    var nbFailures = 0;
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            await Task.Delay(interval, token.Token).ConfigureAwait(false);
                            try
                            {
                                await action(token.Token).ConfigureAwait(false);
                                nbFailures = 0;
                            }
                            catch (Exception x)
                            {
                                MetricsErrorHandler.Handle(x, "Error while executing action scheduler");
                                if (toleratedConsecutiveFailures >= 0)
                                {
                                    nbFailures++;
                                    if (nbFailures > toleratedConsecutiveFailures)
                                    {
                                        token.Cancel();
                                    }
                                }
                            }
                        }
                        catch (TaskCanceledException)
                        {
                        }
                    }
                }, token.Token);
        }

        public void Stop()
        {
            if (token != null)
            {
                token.Cancel();
            }
        }

        public void Dispose()
        {
            if (token != null)
            {
                token.Cancel();
                token.Dispose();
            }
        }

        private CancellationTokenSource token;
        private readonly int toleratedConsecutiveFailures;
    }
}