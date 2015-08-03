﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommonTypes;

namespace Timetable2
{
    using Partial = Tuple<int, int, Timetable, Talks, Talks, Talks>;

    internal static class Program
    {
        private static void Main()
        {
            var timetables = Test();
            Console.WriteLine("Number of timetables found: {0}", timetables.AsImmutableList().Count());
        }

        private static Timetables Test()
        {
            var c1 = new Talk(1);
            var c2 = new Talk(2);
            var c3 = new Talk(3);
            var c4 = new Talk(4);
            var cs = new Talks(c1, c2, c3, c4);
            var testPersons = new[]
            {
                new Person("P", new Talks(c1, c2)),
                new Person("Q", new Talks(c2, c3)),
                new Person("R", new Talks(c3, c4)),
                new Person("S", new Talks(c1, c4))
            };
            return Timetable(testPersons, cs, 2, 2);
        }

        private static Timetables Timetable(IEnumerable<Person> people, Talks allTalks, int maxTrack, int maxSlot)
        {
            var emptysoln = Tuple.Create(0, 0, new Timetable(), new Talks(), allTalks, allTalks);

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
                    return new[] {Tuple.Create(slotNo + 1, 0, new Timetable(slot, slots), new Talks(), talks, talks)};

                return Selects(slotTalks.AsImmutableList()).Select(selectsTuple =>
                {
                    var t = selectsTuple.Item1;
                    var ts = selectsTuple.Item2;
                    Talks clashesWithT;
                    if (!clashes.TryGetValue(t, out clashesWithT)) clashesWithT = new Talks();
                    var slotTalks2 = new Talks(ts.Except(clashesWithT.AsImmutableList()));
                    var talks2 = new Talks(talks.AsImmutableList().Where(talk => talk != t));
                    return Tuple.Create(slotNo, trackNo + 1, slots, new Talks(t, slot), slotTalks2, talks2);
                });
            };

            return new Timetables(ParSearch(finished, refine, emptysoln));
        }

        private static IEnumerable<TSolution> ParSearch<TPartial, TSolution>(
            Func<TPartial, TSolution> finished,
            Func<TPartial, IEnumerable<TPartial>> refine,
            TPartial emptysoln) where TSolution : class
        {
            Func<TPartial, IEnumerable<TSolution>> generate = null;

            //generate = @partial =>
            //{
            //    return finished(@partial) != null
            //        ? new[] {finished(@partial)}
            //        : refine(@partial).AsParallel().SelectMany(p => generate(p)).AsEnumerable();
            //};

            generate = @partial =>
            {
                return finished(@partial) != null
                    ? new[] {finished(@partial)}
                    : refine(@partial)
                        .AsParallel()
                        .Select(p => generate(p))
                        .AsSequential()
                        // ReSharper disable once PossibleMultipleEnumeration
                        .SelectMany(s => s);
            };

            return generate(emptysoln);
        }

        private static IEnumerable<TSolution> Search<TPartial, TSolution>(
            Func<TPartial, TSolution> finished,
            Func<TPartial, IEnumerable<TPartial>> refine,
            TPartial emptysoln) where TSolution : class
        {
            Func<TPartial, IEnumerable<TSolution>> generate = null;

                // ReSharper disable once AssignNullToNotNullAttribute
            generate = @partial => finished(@partial) != null
                ? new[] { finished(@partial) }
                : refine(@partial).SelectMany(generate);

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
