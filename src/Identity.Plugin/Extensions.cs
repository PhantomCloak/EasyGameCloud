using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Plugin
{
    public static class Extensions
    {
        public static T Random<T>(this IEnumerable<T> instance)
        {
            var rnd = new Random();
            var enumerable = instance.ToList();
            var foo = enumerable[rnd.Next(0, enumerable.Count)];
            return foo;
        }
    }
}