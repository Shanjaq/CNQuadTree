/// <summary>
/// TupleExtensions v1.0.0 by Christian Chomiak, christianchomiak@gmail.com
/// 
/// Some functions to help find the Min and Max values of a Tuple.
/// 
/// Only works for:
///     * Tuples where all elements have the same type.
///     * The elements of the tuples are comparable.
/// </summary>

using System;
using System.Drawing;
using System.Collections.Generic;

namespace IterTools
{
    public static class EnumerableExtensions
    {
        public static TSource AggregateWhile<TSource>(this IEnumerable<TSource> source,
                                                 Func<TSource, TSource, TSource> func,
                                                 Func<TSource, bool> predicate)
        {
            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                TSource result = e.Current;
                TSource tmp = default(TSource);
                while (e.MoveNext() && predicate(tmp = func(result, e.Current)))
                    result = tmp;
                return result;
            }
        }
    }
}