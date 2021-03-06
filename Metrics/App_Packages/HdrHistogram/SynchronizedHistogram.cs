// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo

using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace HdrHistogram
{
    /// <summary>
    /// <h3>An integer values High Dynamic Range (HDR) Histogram that is synchronized as a whole</h3>
    /// 
    /// A {@link SynchronizedHistogram} is a variant of {@link Histogram} that is
    /// synchronized as a whole, such that queries, copying, and addition operations are atomic with relation to
    /// modification on the {@link SynchronizedHistogram}, and such that external accessors (e.g. iterations on the
    /// histogram data) that synchronize on the {@link SynchronizedHistogram} instance can safely assume that no
    /// modifications to the histogram data occur within their synchronized block.
    /// 
    /// It is important to note that synchronization can result in blocking recoding calls. If non-blocking recoding
    /// operations are required, consider using {@link ConcurrentHistogram}, {@link AtomicHistogram}, or (recommended)
    /// {@link Recorder} or {@link org.HdrHistogram.SingleWriterRecorder} which were intended for concurrent operations.
    /// 
    /// See package description for {@link org.HdrHistogram} and {@link org.HdrHistogram.Histogram} for more details.
    /// </summary>
    internal class SynchronizedHistogram : Histogram
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal override long getCountAtIndex(int index)
        {
            return counts[NormalizeIndex(index, normalizingIndexOffset, countsArrayLength)];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override long getCountAtNormalizedIndex(int index)
        {
            return counts[index];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void incrementCountAtIndex(int index)
        {
            counts[NormalizeIndex(index, normalizingIndexOffset, countsArrayLength)]++;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void addToCountAtIndex(int index, long value)
        {
            counts[NormalizeIndex(index, normalizingIndexOffset, countsArrayLength)] += value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void setCountAtIndex(int index, long value)
        {
            counts[NormalizeIndex(index, normalizingIndexOffset, countsArrayLength)] = value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void setCountAtNormalizedIndex(int index, long value)
        {
            counts[index] = value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override int getNormalizingIndexOffset()
        {
            return normalizingIndexOffset;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void setNormalizingIndexOffset(int normalizingIndexOffset)
        {
            this.normalizingIndexOffset = normalizingIndexOffset;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void shiftNormalizingIndexByOffset(int offsetToAdd, bool lowestHalfBucketPopulated)
        {
            nonConcurrentNormalizingIndexShift(offsetToAdd, lowestHalfBucketPopulated);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal override void clearCounts()
        {
            Array.Clear(counts, 0, counts.Length);
            totalCount = 0;
        }

        public new void add(AbstractHistogram otherHistogram)
        {
            // Synchronize add(). Avoid deadlocks by synchronizing in order of construction identity count.
            if (Identity < otherHistogram.Identity)
            {
                lock (this)
                {
                    lock (otherHistogram)
                    {
                        base.add(otherHistogram);
                    }
                }
            }
            else
            {
                lock (otherHistogram)
                {
                    lock (this)
                    {
                        base.add(otherHistogram);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal override void shiftValuesLeft(int numberOfBinaryOrdersOfMagnitude)
        {
            base.shiftValuesLeft(numberOfBinaryOrdersOfMagnitude);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal override void shiftValuesRight(int numberOfBinaryOrdersOfMagnitude)
        {
            base.shiftValuesRight(numberOfBinaryOrdersOfMagnitude);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override AbstractHistogram copy()
        {
            SynchronizedHistogram copy;
            lock (this)
            {
                copy = new SynchronizedHistogram(this);
            }
            copy.add(this);
            return copy;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override AbstractHistogram copyCorrectedForCoordinatedOmission(long expectedIntervalBetweenValueSamples)
        {
            lock (this)
            {
                SynchronizedHistogram toHistogram = new SynchronizedHistogram(this);
                toHistogram.addWhileCorrectingForCoordinatedOmission(this, expectedIntervalBetweenValueSamples);
                return toHistogram;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override long getTotalCount()
        {
            return totalCount;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void setTotalCount(long totalCount)
        {
            this.totalCount = totalCount;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void incrementTotalCount()
        {
            totalCount++;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void addToTotalCount(long value)
        {
            totalCount += value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void updatedMaxValue(long maxValue)
        {
            if (maxValue > getMaxValue())
            {
                base.updatedMaxValue(maxValue);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void updateMinNonZeroValue(long minNonZeroValue)
        {
            if (minNonZeroValue < getMinNonZeroValue())
            {
                base.updateMinNonZeroValue(minNonZeroValue);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal override int _getEstimatedFootprintInBytes()
        {
            return (512 + (8 * counts.Length));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal override void resize(long newHighestTrackableValue)
        {
            int oldNormalizedZeroIndex = NormalizeIndex(0, normalizingIndexOffset, countsArrayLength);

            establishSize(newHighestTrackableValue);

            int countsDelta = countsArrayLength - counts.Length;
            Array.Resize(ref counts, countsArrayLength);

            if (oldNormalizedZeroIndex != 0)
            {
                // We need to shift the stuff from the zero index and up to the end of the array:
                int newNormalizedZeroIndex = oldNormalizedZeroIndex + countsDelta;
                int lengthToCopy = (countsArrayLength - countsDelta) - oldNormalizedZeroIndex;
                Array.Copy(counts, oldNormalizedZeroIndex, counts, newNormalizedZeroIndex, lengthToCopy);
            }
        }

        /**
     * Construct an auto-resizing SynchronizedHistogram with a lowest discernible value of 1 and an auto-adjusting
     * highestTrackableValue. Can auto-resize up to track values up to (Long.MAX_VALUE / 2).
     *
     * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
     *                                       decimal digits to which the histogram will maintain value resolution
     *                                       and separation. Must be a non-negative integer between 0 and 5.
     */

        public SynchronizedHistogram(int numberOfSignificantValueDigits)
            : base(numberOfSignificantValueDigits)
        { }

        /**
     * Construct a SynchronizedHistogram given the Highest value to be tracked and a number of significant decimal digits. The
     * histogram will be constructed to implicitly track (distinguish from 0) values as low as 1.
     *
     * @param highestTrackableValue The highest value to be tracked by the histogram. Must be a positive
     *                              integer that is {@literal >=} 2.
     * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
     *                                       decimal digits to which the histogram will maintain value resolution
     *                                       and separation. Must be a non-negative integer between 0 and 5.
     */

        public SynchronizedHistogram(long highestTrackableValue, int numberOfSignificantValueDigits)
            : base(highestTrackableValue, numberOfSignificantValueDigits)
        { }

        /**
     * Construct a SynchronizedHistogram given the Lowest and Highest values to be tracked and a number of significant
     * decimal digits. Providing a lowestDiscernibleValue is useful is situations where the units used
     * for the histogram's values are much smaller that the minimal accuracy required. E.g. when tracking
     * time values stated in nanosecond units, where the minimal accuracy required is a microsecond, the
     * proper value for lowestDiscernibleValue would be 1000.
     *
     * @param lowestDiscernibleValue The lowest value that can be tracked (distinguished from 0) by the histogram.
     *                               Must be a positive integer that is {@literal >=} 1. May be internally rounded
     *                               down to nearest power of 2.
     * @param highestTrackableValue The highest value to be tracked by the histogram. Must be a positive
     *                              integer that is {@literal >=} (2 * lowestDiscernibleValue).
     * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
     *                                       decimal digits to which the histogram will maintain value resolution
     *                                       and separation. Must be a non-negative integer between 0 and 5.
     */

        public SynchronizedHistogram(long lowestDiscernibleValue, long highestTrackableValue, int numberOfSignificantValueDigits)
            : base(lowestDiscernibleValue, highestTrackableValue, numberOfSignificantValueDigits)
        {
        }

        /**
     * Construct a histogram with the same range settings as a given source histogram,
     * duplicating the source's start/end timestamps (but NOT it's contents)
     * @param source The source histogram to duplicate
     */

        public SynchronizedHistogram(AbstractHistogram source)
            : base(source)
        {
        }
    }
}