#if !DOTNET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Linq
{
    /// <summary>
    /// Class for the really simple bits of LINQ to Objects.
    /// These are not meant to be performant or robust, just good enough
    /// to help testing.
    /// </summary>
    public static class SimpleLinq
    {
        public static IEnumerable<T> Reverse<T>(this IEnumerable<T> source)
        {
            List<T> list = new List<T>(source);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                yield return list[i];
            }
        }

        public static bool SequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            List<T> first = new List<T>(source);
            List<T> second = new List<T>(other);
            if (first.Count != second.Count)
            {
                return false;
            }
            for (int i = 0; i < first.Count; i++)
            {
                if (!object.Equals(first[i], second[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static List<T> ToList<T>(this IEnumerable<T> source)
        {
            return new List<T>(source);
        }

        public static int Count<T>(this IEnumerable<T> source)
        {
            return source.ToList().Count;
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> source, int amount)
        {
            int taken = 0;
            foreach (T t in source)
            {
                if (taken >= amount)
                {
                    yield break;
                }
                yield return t;
                taken++;
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source,
                                                                    Func<TSource, TResult> projection)
        {
            foreach (TSource item in source)
            {
                yield return projection(item);
            }
        }
    }
}
#endif