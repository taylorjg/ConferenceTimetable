using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommonTypes;

namespace Timetable1
{
    internal static class Program
    {
        private static void Main()
        {
            var timetables = Test();
            Console.WriteLine("Number of timetables found: {0}", timetables.AsEnumerable().Count());
        }

        private static Timetables Test()
        {
            var c1 = new Talk(1);
            var c2 = new Talk(2);
            var c3 = new Talk(3);
            var c4 = new Talk(4);
            var cs = new Talks(ImmutableList.Create(c1, c2, c3, c4));
            var testPersons = new[]
            {
                new Person("P", new Talks(ImmutableList.Create(c1, c2))),
                new Person("Q", new Talks(ImmutableList.Create(c2, c3))),
                new Person("R", new Talks(ImmutableList.Create(c3, c4))),
                new Person("S", new Talks(ImmutableList.Create(c1, c4)))
            };
            return Timetable(testPersons, cs, 2, 2);
        }

        private static Timetables Timetable(IEnumerable<Person> people, Talks allTalks, int maxTrack, int maxSlot)
        {
            var clashes = BuildClashesMap(people);

            Func<int, int, Timetable, Talks, Talks, Talks, Timetables> generate = null;

            generate = (slotNo, trackNo, slots, slot, slotTalks, talks) =>
            {
                if (slotNo == maxSlot)
                    return new Timetables(ImmutableList.Create(slots));

                if (trackNo == maxTrack)
                {
                    return generate(slotNo + 1, 0, new Timetable(ImmutableList.CreateRange(new[]{slot}.Concat(slots.AsEnumerable()))), new Talks(ImmutableList<Talk>.Empty), talks, talks);
                }

                return new Timetables(ImmutableList.CreateRange(Selects(slotTalks.AsEnumerable().ToImmutableList()).SelectMany(tuple =>
                {
                    var t = tuple.Item1;
                    var ts = tuple.Item2;
                    Talks clashesWithT;
                    if (!clashes.TryGetValue(t, out clashesWithT))
                    {
                        clashesWithT = new Talks(ImmutableList<Talk>.Empty);
                    }
                    var slotTalks2 = new Talks(ImmutableList.CreateRange(ts.Except(clashesWithT.AsEnumerable())));
                    var talks2 = new Talks(ImmutableList.CreateRange(talks.AsEnumerable().Where(x => x != t)));
                    return generate(slotNo, trackNo + 1, slots, new Talks(ImmutableList.CreateRange(new[]{t}.Concat(slot.AsEnumerable()))), slotTalks2, talks2).AsEnumerable();
                })));
            };

            return generate(0, 0, new Timetable(ImmutableList<Talks>.Empty), new Talks(ImmutableList<Talk>.Empty), allTalks, allTalks);
        }

        private static Dictionary<Talk, Talks> BuildClashesMap(IEnumerable<Person> people)
        {
            return people
                .SelectMany(s => Selects(s.Talks.AsEnumerable().ToImmutableList()))
                .ToLookup(
                    x => x.Item1,
                    x => x.Item2.ToImmutableList())
                .ToDictionary(
                    x => x.Key,
                    x => new Talks(ImmutableList.CreateRange(x.SelectMany(ts => ts).Distinct())));
        }

        private static IEnumerable<Tuple<T, IEnumerable<T>>> Selects<T>(IImmutableList<T> xs)
        {
            return xs.Select(x => Tuple.Create(x, xs.Except(new[] { x })));
        }
    }
}
