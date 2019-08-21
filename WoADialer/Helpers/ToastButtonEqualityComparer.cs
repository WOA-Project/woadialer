using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoADialer.Helpers
{
    public sealed class ToastButtonEqualityComparer : IEqualityComparer<ToastButton>
    {
        public static ToastButtonEqualityComparer EqualityComparer { get; } = new ToastButtonEqualityComparer();

        private ToastButtonEqualityComparer()
        {

        }

        public bool Equals(ToastButton x, ToastButton y)
        {
            return (x == null && y == null) || (x != null && y != null && x.Arguments == y.Arguments && x.Content == y.Content);
        }

        public int GetHashCode(ToastButton obj)
        {
            return obj == null ? 0 : obj.Arguments.GetHashCode() ^ obj.Content.GetHashCode();
        }
    }
}
