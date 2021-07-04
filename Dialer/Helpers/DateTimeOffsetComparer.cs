using System;
using System.Collections.Generic;

namespace Dialer.Helpers
{
    public sealed class DateTimeOffsetComparer : IComparer<DateTimeOffset?>
    {
        public static DateTimeOffsetComparer Comparer { get; } = new DateTimeOffsetComparer();

        public int Compare(DateTimeOffset? x, DateTimeOffset? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null)
            {
                return 1;
            }
            else if (y == null)
            {
                return -1;
            }
            else
            {
                return DateTimeOffset.Compare(x.Value, y.Value);
            }
        }
    }
}
