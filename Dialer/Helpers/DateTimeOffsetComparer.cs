using System;
using System.Collections.Generic;

namespace Dialer.Helpers
{
    public sealed class DateTimeOffsetComparer : IComparer<DateTimeOffset?>
    {
        public static DateTimeOffsetComparer Comparer { get; } = new DateTimeOffsetComparer();

        public int Compare(DateTimeOffset? x, DateTimeOffset? y)
        {
            return x == null && y == null ? 0 : x == null ? 1 : y == null ? -1 : DateTimeOffset.Compare(x.Value, y.Value);
        }
    }
}
