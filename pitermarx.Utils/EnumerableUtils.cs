using System;
using System.Collections.Generic;
using System.Linq;

namespace pitermarx.Utils
{
    public static class EnumerableUtils
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> tree, Func<T, IEnumerable<T>> getBranches)
        {
            var enumerable = tree as T[] ?? tree.ToArray();
            return enumerable.Concat(enumerable.SafeSelectMany(b => getBranches(b)).Flatten(getBranches));
        }

        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            var list = source.ToList();
            while (list.Any())
            {
                yield return list.Take(chunksize);
                list = list.Skip(chunksize).ToList();
            }
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
            => source?.Where(t => t != null) ?? Enumerable.Empty<T>();

        public static IEnumerable<TOut> SafeSelectMany<T, TOut>(this IEnumerable<T> source, Func<T, IEnumerable<TOut>> selector)
            => source.NotNull().SelectMany(t => selector(t).NotNull());
    }
}