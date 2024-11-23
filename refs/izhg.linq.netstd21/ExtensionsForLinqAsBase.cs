using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Linq
{
    public static class ExtensionsForLinqAsBase
    {
        public static bool TryFindFirst<T>(this IEnumerable<T> e, Func<T, bool> predictate, out T result)
        {
            result = e.FirstOrDefault(predictate);
            return result != null;
        }
        public static bool TryFindFirst<T>(this IEnumerable<T> e, out T result)
        {
            result = e.FirstOrDefault();
            return result != null;
        }
    }
}
