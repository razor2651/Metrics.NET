﻿using System;
using System.Diagnostics;

using Metrics.MetricData;

namespace Metrics.PerfCounters
{
    public class PerformanceCounterGauge : MetricValueProvider<double>
    {
        public PerformanceCounterGauge(string category, string counter)
            : this(category, counter, instance : null)
        {
        }

        public PerformanceCounterGauge(string category, string counter, string instance)
        {
            try
            {
                performanceCounter = instance == null ? new PerformanceCounter(category, counter, true) : new PerformanceCounter(category, counter, instance, true);
                Metric.Internal.Counter("Performance Counters", Unit.Custom("Perf Counters")).Increment();
            }
            catch (Exception x)
            {
                const string message = "Error reading performance counter data." +
                                       " Make sure the user has access to the performance counters. The user needs to be either Admin or belong to Performance Monitor user group.";
                MetricsErrorHandler.Handle(x, message);
            }
        }

        public double GetValue(bool resetMetric = false)
        {
            return Value;
        }

        public double Value
        {
            get
            {
                try
                {
                    return performanceCounter?.NextValue() ?? double.NaN;
                }
                catch (Exception x)
                {
                    const string message = "Error reading performance counter data. " +
                                           "Make sure the user has access to the performance counters. The user needs to be either Admin or belong to Performance Monitor user group.";
                    MetricsErrorHandler.Handle(x, message);
                    return double.NaN;
                }
            }
        }

        private readonly PerformanceCounter performanceCounter;
    }
}