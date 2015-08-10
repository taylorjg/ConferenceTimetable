using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CommonTypes;

namespace Timetable2
{
    internal static class Program
    {
        private static void Main()
        {
            Utils.RunWithStats(() =>
            {
                var testData = Utils.GetHardCodedTestData();
                Utils.DumpTestData(testData.Item1, testData.Item2);

                const int maxTrack = 3;
                const int maxSlot = 4;

                var timetables = Timetable(testData.Item1, testData.Item2, maxTrack, maxSlot);

                var count = 0;
                const int numTimetablesToDump = 4;

                using (var e = timetables.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        if (count++ <= numTimetablesToDump)
                        {
                            Utils.DumpTimetable(e.Current);
                            Console.WriteLine();
                        }
                    }
                }

                Console.WriteLine("Number of timetables found: {0}", count);
            });
        }

        private static IEnumerable<Timetable> Timetable(IEnumerable<Person> people, Talks allTalks, int maxTrack, int maxSlot)
        {
            var emptysoln = new Partial(0, 0, new Timetable(), new Talks(), allTalks, allTalks);

            Func<Partial, Timetable> finished = finishedTuple =>
            {
                var slotNo = finishedTuple.Item1;
                var slots = finishedTuple.Item3;
                return slotNo == maxSlot ? slots : null;
            };

            var clashes = BuildClashesMap(people);

            Func<Partial, IEnumerable<Partial>> refine = refineTuple =>
            {
                var slotNo = refineTuple.Item1;
                var trackNo = refineTuple.Item2;
                var slots = refineTuple.Item3;
                var slot = refineTuple.Item4;
                var slotTalks = refineTuple.Item5;
                var talks = refineTuple.Item6;

                if (trackNo == maxTrack)
                    return new[] {new Partial(slotNo + 1, 0, new Timetable(slot, slots), new Talks(), talks, talks)};

                return Selects(slotTalks.AsImmutableList()).Select(selectsTuple =>
                {
                    var t = selectsTuple.Item1;
                    var ts = selectsTuple.Item2;
                    Talks clashesWithT;
                    if (!clashes.TryGetValue(t, out clashesWithT)) clashesWithT = new Talks();
                    var slotTalks2 = new Talks(ts.Except(clashesWithT.AsImmutableList()));
                    var talks2 = new Talks(talks.AsImmutableList().Where(talk => talk != t));
                    return new Partial(slotNo, trackNo + 1, slots, new Talks(t, slot), slotTalks2, talks2);
                });
            };

            return ParSearch(4, finished, refine, emptysoln);
            //return Search(finished, refine, emptysoln);
        }

        private static IEnumerable<TSolution> ParSearch<TPartial, TSolution>(
            int maxDepth,
            Func<TPartial, TSolution> finished,
            Func<TPartial, IEnumerable<TPartial>> refine,
            TPartial emptysoln) where TSolution : class
        {
            Func<int, TPartial, IEnumerable<TSolution>> generate = null;

            // ReSharper disable once AssignNullToNotNullAttribute
            generate = (d, @partial) =>
            {
                if (d >= maxDepth) return Search(finished, refine, @partial);

                var soln = finished(@partial);

                if (soln != null) return Enumerable.Repeat(soln, 1);

                var solnss = new List<List<IEnumerable<TSolution>>>();
                var lockObject = new object();

                Parallel.ForEach(
                    refine(@partial),
                    () =>
                        new List<IEnumerable<TSolution>>(),
                    (p, _, local) =>
                    {
                        local.Add(generate(d + 1, p).ToList());
                        //local.Add(generate(d + 1, p).ToChunckedList(4));
                        return local;
                    },
                    local =>
                    {
                        lock (lockObject)
                        {
                            solnss.Add(local);
                        }
                    });

                return ConcatAll(ConcatAll(solnss));
            };

            return generate(0, emptysoln);
        }

        private static IEnumerable<TSolution> Search<TPartial, TSolution>(
            Func<TPartial, TSolution> finished,
            Func<TPartial, IEnumerable<TPartial>> refine,
            TPartial emptysoln) where TSolution : class
        {
            Func<TPartial, IEnumerable<TSolution>> generate = null;

            // ReSharper disable once AssignNullToNotNullAttribute
            generate = @partial =>
            {
                var soln = finished(@partial);
                return soln != null
                    ? Enumerable.Repeat(soln, 1)
                    : ConcatAll(refine(@partial).Select(generate));
            };

            return generate(emptysoln);
        }

        private static Dictionary<Talk, Talks> BuildClashesMap(IEnumerable<Person> people)
        {
            return people
                .SelectMany(s => Selects(s.Talks.AsImmutableList()))
                .ToLookup(
                    x => x.Item1,
                    x => x.Item2.ToImmutableList())
                .ToDictionary(
                    x => x.Key,
                    x => new Talks(x.SelectMany(ts => ts).Distinct()));
        }

        private static IEnumerable<Tuple<T, IEnumerable<T>>> Selects<T>(IImmutableList<T> xs)
        {
            return xs.Select(x => Tuple.Create(x, xs.Except(new[] { x })));
        }

        private static IEnumerable<T> ConcatAll<T>(IEnumerable<IEnumerable<T>> sources)
        {
            foreach (var source in sources)
                using (var e = source.GetEnumerator())
                    while (e.MoveNext())
                        yield return e.Current;
        }
    }
}
