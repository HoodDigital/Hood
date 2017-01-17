using CodeComb.HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        private delegate Func<A, R> Recursive<A, R>(Recursive<A, R> r);
        private static Func<A, R> Y<A, R>(Func<Func<A, R>, Func<A, R>> f)
        {
            Recursive<A, R> rec = r => a => f(r(r))(a); return rec(rec);
        }

        public static IEnumerable<HtmlNode> Traverse(this IEnumerable<HtmlNode> source, Func<HtmlNode, bool> predicate)
        {
            var traverse = EnumerableExtensions.Y<IEnumerable<HtmlNode>, IEnumerable<HtmlNode>>(
                f => items =>
                {
                    var r = new List<HtmlNode>(items.Where(predicate));
                    r.AddRange(items.SelectMany(i => f(i.ChildNodes.AsEnumerable())));
                    return r;
                });

            return traverse(source);
        }
    }
}
