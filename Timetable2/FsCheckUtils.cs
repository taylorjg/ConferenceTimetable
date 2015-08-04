﻿using System.Collections.Generic;
using System.Linq;
using FsCheck;

namespace Timetable2
{
    public static class FsCheckUtils
    {
        public static Gen<IList<T>> PickValues<T>(int n, IEnumerable<T> l)
        {
            return PickValues(n, l.ToArray());
        }

        private static Gen<IList<T>> PickValues<T>(int n, params T[] l)
        {
            var allIdxs = Enumerable.Range(0, l.Length).ToArray();
            return Gen.Elements(allIdxs).ListOf(n)
                .Where(idxs => idxs.Distinct().Count() == n)
                .Select(idxs => idxs.Select(idx => l[idx]).ToList() as IList<T>);
        }
    }
}
