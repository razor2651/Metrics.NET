using System;
using System.Reflection;

using FluentAssertions;

using Metrics.Core;
using Metrics.Sampling;

using NUnit.Framework;

namespace Metrics.Tests.Sampling
{
    public class DefaultSamplingTypeTests
    {
        private static Reservoir GetReservoir(HistogramMetric histogram)
        {
            return reservoirField.GetValue(histogram) as Reservoir;
        }

        [Test]
        public void SamplingType_CanUseConfiguredDefaultSamplingType()
        {
            GetReservoir(new HistogramMetric()).Should().BeOfType<ExponentiallyDecayingReservoir>();

            Metric.Config.WithDefaultSamplingType(SamplingType.HighDynamicRange);

            GetReservoir(new HistogramMetric()).Should().BeOfType<HdrHistogramReservoir>();

            Metric.Config.WithDefaultSamplingType(SamplingType.LongTerm);

            GetReservoir(new HistogramMetric()).Should().BeOfType<UniformReservoir>();
        }

        [Test]
        public void SamplingType_SettingDefaultValueMustBeConcreteValue()
        {
            Assert.Throws<ArgumentException>(() => { Metric.Config.WithDefaultSamplingType(SamplingType.Default); });
        }

        private static readonly FieldInfo reservoirField = typeof(HistogramMetric).GetField("reservoir", BindingFlags.Instance | BindingFlags.NonPublic);
    }
}