using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer
{
    public static class Extension
    {
        public static List<TSource> DistinctProperty<TSource, TKey>(this List<TSource> enumerable, Func<TSource, TKey> keySelector)
        {
            return enumerable.GroupBy(keySelector).Select(x => x.First()).ToList();
        }
    }
}