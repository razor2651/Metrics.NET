﻿using System;
using System.Collections.Generic;

using Metrics.MetricData;

namespace Metrics.Core
{
    public sealed class DefaultRegistryDataProvider : RegistryDataProvider
    {
        public DefaultRegistryDataProvider(
            Func<IEnumerable<GaugeValueSource>> gauges,
            Func<IEnumerable<CounterValueSource>> counters,
            Func<IEnumerable<MeterValueSource>> meters,
            Func<IEnumerable<HistogramValueSource>> histograms,
            Func<IEnumerable<TimerValueSource>> timers)
        {
            this.gauges = gauges;
            this.counters = counters;
            this.meters = meters;
            this.histograms = histograms;
            this.timers = timers;
        }

        public IEnumerable<GaugeValueSource> Gauges { get { return gauges(); } }
        public IEnumerable<CounterValueSource> Counters { get { return counters(); } }
        public IEnumerable<MeterValueSource> Meters { get { return meters(); } }
        public IEnumerable<HistogramValueSource> Histograms { get { return histograms(); } }
        public IEnumerable<TimerValueSource> Timers { get { return timers(); } }
        private readonly Func<IEnumerable<GaugeValueSource>> gauges;
        private readonly Func<IEnumerable<CounterValueSource>> counters;
        private readonly Func<IEnumerable<MeterValueSource>> meters;
        private readonly Func<IEnumerable<HistogramValueSource>> histograms;
        private readonly Func<IEnumerable<TimerValueSource>> timers;
    }
}