using System.Collections.Generic;
using System.Linq;

namespace Timetable2
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ToChunckedList<T>(this IEnumerable<T> source, int n)
        {
            using (var e = source.GetEnumerator())
            {
                for (var done = false; !done;)
                {
                    var chunk = new List<T>();

                    foreach (var _ in Enumerable.Range(1, n))
                    {
                        if (!e.MoveNext())
                        {
                            done = true;
                            break;
                        }

                        chunk.Add(e.Current);
                    }

                    foreach (var item in chunk)
                        yield return item;
                }
            }
        }
    }
}
