using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Utilities
{
    static class Store
    {
        public static Dictionary<string, object> store = new();

        public static T? GetStoreItem<T>(string key)
        {
            if (store.TryGetValue(key, out var obj))
                return (T)obj;
            throw new Exception("Key " + key + " not found in store!");
        }
    }

    internal static class IEnumerableExt
    {
        public static IEnumerable<T> wj_Store_SaveAsArray<T>(this IEnumerable<T> source, string key)
        {
            Store.store.Add(key, source.ToArray());

            foreach (T element in source)
            {
                yield return element;
            }
        }

        public static T[]? wj_Store_LoadArray<T>(this IEnumerable<T> source, string key)
        {
            return Store.GetStoreItem<T[]>(key);
        }

        public static IEnumerable<(T, IEnumerable<T>)> wj_Store_IterOverArray<T>(this IEnumerable<T> source, string key)
        {
            T[]? si = Store.GetStoreItem<T[]>(key);

            foreach (T element in source)
            {
                yield return (element, source);
            }
        }

        public static IEnumerable<T> wj_Iter_OverRange<T>(this List<T> source, int startInd, int len)
        {
            for (int i = startInd; i < source.Count && i < startInd + len; ++i)
            {
                yield return source[i];
            }
        }

        public static IEnumerable<T> wj_Iter_OverRange<T>(this T[] source, int startInd, int len)
        {
            for (int i = startInd; i < source.Length && i < startInd + len; ++i)
            {
                yield return source[i];
            }
        }

        public static IEnumerable<T> wj_IterTransposed<T>(this List<T> source, int width)
        {
            int totalX = width;
            int totalY = source.Count / width;

            for (int x = 0; x < totalX; ++x)
            {
                for (int y = 0; y < totalY; ++x)
                {
                    yield return source[x + y * width];
                }
            }
        }

        public static IEnumerable<T> wj_IterTransposed<T>(this T[] source, int width)
        {
            int totalX = width;
            int totalY = source.Length / width;

            for (int x = 0; x < totalX; ++x)
            {
                for (int y = 0; y < totalY; ++x)
                {
                    yield return source[x + y * width];
                }
            }
        }

        public static T wj_GetPos<T>(this T[] source, int width, int x, int y)
        {
            return source[x + y * width];
        }

        public static T wj_GetPos<T>(this List<T> source, int width, int x, int y)
        {
            return source[x + y * width];
        }

        public static Y wj_SelectSelf<T, Y>(this IEnumerable<T> source, Func<IEnumerable<T>, Y> selector)
        {
            return selector(source);
        }

        public static IEnumerable<T> wj_SelectSelf<T>(this IEnumerable<T> source, Func<IEnumerable<T>, IEnumerable<T>> selector)
        {
            return selector(source);
        }

        public static T wj_SelectSelf<T>(this IEnumerable<T> source, Func<IEnumerable<T>, T> selector)
        {
            return selector(source);
        }

        public static IEnumerable<T> wj_SelectSelfAsList<T>(this IEnumerable<T>? source, Func<List<T>?, IEnumerable<T>> selector)
        {
            return selector(source == null? null: source.ToList());
        }

        public static T wj_SelectSelfAsList<T>(this IEnumerable<T> source, Func<List<T>, T> selector)
        {
            return selector(source.ToList());
        }

        public static IEnumerable<T> wj_Log<T>(this IEnumerable<T> source, string msg)
        {
            Console.WriteLine(msg);
            return source;
        }


        //used

        public static TSource wj_ElementAt<TSource>(this IEnumerable<TSource> source, int x, int y, int width)
        {
            return source.ElementAt(x + (y * width));
        }
    }
}
