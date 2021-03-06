﻿using System;
using System.Collections.Generic;

namespace Metrics.MetricData
{
    public struct CounterValue
    {
        internal CounterValue(long count)
            : this(count, noItems)
        {
        }

        public CounterValue(long count, SetItem[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Count = count;
            Items = items;
        }

        public struct SetItem
        {
            public SetItem(string item, long count, double percent)
            {
                Item = item;
                Count = count;
                Percent = percent;
            }

            /// <summary>
            ///     Registered item name.
            /// </summary>
            public readonly string Item;

            /// <summary>
            ///     Specific count for this item.
            /// </summary>
            public readonly long Count;

            /// <summary>
            ///     Percent of this item from the total count.
            /// </summary>
            public readonly double Percent;
        }

        private static readonly SetItem[] noItems = new SetItem[0];

        public static readonly IComparer<SetItem> SetItemComparer = Comparer<SetItem>.Create((x, y) =>
            {
                var percent = Comparer<double>.Default.Compare(x.Percent, y.Percent);
                return percent == 0 ? Comparer<string>.Default.Compare(x.Item, y.Item) : percent;
            });

        /// <summary>
        ///     Total count of the counter instance.
        /// </summary>
        public readonly long Count;

        /// <summary>
        ///     Separate counters for each registered set item.
        /// </summary>
        public readonly SetItem[] Items;
    }

    /// <summary>
    ///     Combines the value for a counter with the defined unit for the value.
    /// </summary>
    public sealed class CounterValueSource : MetricValueSource<CounterValue>
    {
        public CounterValueSource(string name, MetricValueProvider<CounterValue> value, Unit unit, MetricTags tags)
            : base(name, value, unit, tags)
        {
        }
    }
}