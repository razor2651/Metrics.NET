// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo

// ReSharper disable once CheckNamespace
namespace HdrHistogram
{

    /// <summary>
    /// Represents a value point iterated through in a Histogram, with associated stats.
    /// <list type="bullet">
    /// <item><b><code>valueIteratedTo</code></b> : The actual value level that was iterated to by the iterator</item>
    /// <item><b><code>prevValueIteratedTo</code></b> : The actual value level that was iterated from by the iterator</item>
    /// <item><b><code>countAtValueIteratedTo</code></b> : The count of recorded values in the histogram that
    /// exactly match this [lowestEquivalentValue(valueIteratedTo)...highestEquivalentValue(valueIteratedTo)] value
    /// range.</item>
    /// <item><b><code>countAddedInThisIterationStep</code></b> : The count of recorded values in the histogram that
    /// were added to the totalCountToThisValue (below) as a result on this iteration step. Since multiple iteration
    /// steps may occur with overlapping equivalent value ranges, the count may be lower than the count found at
    /// the value (e.g. multiple linear steps or percentile levels can occur within a single equivalent value range)</item>
    /// <item><b><code>totalCountToThisValue</code></b> : The total count of all recorded values in the histogram at
    /// values equal or smaller than valueIteratedTo.</item>
    /// <item><b><code>totalValueToThisValue</code></b> : The sum of all recorded values in the histogram at values
    /// equal or smaller than valueIteratedTo.</item>
    /// <item><b><code>percentile</code></b> : The percentile of recorded values in the histogram at values equal
    /// or smaller than valueIteratedTo.</item>
    /// <item><b><code>percentileLevelIteratedTo</code></b> : The percentile level that the iterator returning this
    /// HistogramIterationValue had iterated to. Generally, percentileLevelIteratedTo will be equal to or smaller than
    /// percentile, but the same value point can contain multiple iteration levels for some iterators. E.g. a
    /// PercentileIterator can stop multiple times in the exact same value point (if the count at that value covers a
    /// range of multiple percentiles in the requested percentile iteration points).</item>
    /// </list>
    /// </summary>
    internal class HistogramIterationValue
    {
        private long valueIteratedTo;
        private long valueIteratedFrom;
        private long countAtValueIteratedTo;
        private long countAddedInThisIterationStep;
        private long totalCountToThisValue;
        private long totalValueToThisValue;
        private double percentile;
        private double percentileLevelIteratedTo;
        private double integerToDoubleValueConversionRatio;

        // Set is all-or-nothing to avoid the potential for accidental omission of some values...
        internal void set(long valueIteratedTo, long valueIteratedFrom, long countAtValueIteratedTo,
            long countInThisIterationStep, long totalCountToThisValue, long totalValueToThisValue,
            double percentile, double percentileLevelIteratedTo, double integerToDoubleValueConversionRatio)
        {
            this.valueIteratedTo = valueIteratedTo;
            this.valueIteratedFrom = valueIteratedFrom;
            this.countAtValueIteratedTo = countAtValueIteratedTo;
            countAddedInThisIterationStep = countInThisIterationStep;
            this.totalCountToThisValue = totalCountToThisValue;
            this.totalValueToThisValue = totalValueToThisValue;
            this.percentile = percentile;
            this.percentileLevelIteratedTo = percentileLevelIteratedTo;
            this.integerToDoubleValueConversionRatio = integerToDoubleValueConversionRatio;
        }

        internal void reset()
        {
            valueIteratedTo = 0;
            valueIteratedFrom = 0;
            countAtValueIteratedTo = 0;
            countAddedInThisIterationStep = 0;
            totalCountToThisValue = 0;
            totalValueToThisValue = 0;
            percentile = 0.0;
            percentileLevelIteratedTo = 0.0;
        }

        public long getValueIteratedTo()
        {
            return valueIteratedTo;
        }

        public long getValueIteratedFrom()
        {
            return valueIteratedFrom;
        }

        public long getCountAtValueIteratedTo()
        {
            return countAtValueIteratedTo;
        }

        public long getCountAddedInThisIterationStep()
        {
            return countAddedInThisIterationStep;
        }

        public long getTotalCountToThisValue()
        {
            return totalCountToThisValue;
        }

        public long getTotalValueToThisValue()
        {
            return totalValueToThisValue;
        }

        public double getPercentile()
        {
            return percentile;
        }

        public double getPercentileLevelIteratedTo()
        {
            return percentileLevelIteratedTo;
        }

        public double getIntegerToDoubleValueConversionRatio()
        {
            return integerToDoubleValueConversionRatio;
        }
    }
}
