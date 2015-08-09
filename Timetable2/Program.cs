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
            var testData = Utils.GetHardCodedTestData();
            Utils.DumpTestData(testData.Item1, testData.Item2);
            Utils.RunWithStats(() =>
            {
                var timetables = Timetable(testData.Item1, testData.Item2, 3, 4);
                var count = 0;
                const int numberToPrint = 4;
                using (var e = timetables.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        if (count <= numberToPrint) Utils.DumpTimetable(e.Current);
                        count++;
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

            return ParSearch(3, finished, refine, emptysoln);
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

                var solnss = new List<List<TSolution>>();
                var lockObject = new object();

                Parallel.ForEach(
                    refine(@partial),
                    () =>
                        new List<List<TSolution>>(),
                    (p, _, local) =>
                    {
                        local.Add(generate(d + 1, p).ToList());
                        return local;
                    },
                    local =>
                    {
                        lock (lockObject)
                        {
                            solnss.AddRange(local);
                        }
                    });

                return solnss.SelectMany(x => x);
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
                    ? new[] {soln}
                    : refine(@partial).SelectMany(generate);
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
    }
}
