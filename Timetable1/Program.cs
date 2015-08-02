using System;
using System.Collections.Generic;
using System.Linq;
using Talk = System.Int32;

namespace Timetable1
{
    using Talks = List<Talk>;
    using Timetable = List<List<Talk>>;
    using ClashesMap = Dictionary<Talk, List<Talk>>;

    internal static class Program
    {
        private static void Main(/* string[] args */)
        {
        }

        private static List<Timetable> Timetable(IEnumerable<Person> people, Talks allTalks, int maxTrack, int maxSlot)
        {
            var clashes = BuildClashesMap(people);

            Func<int, int, List<List<Talk>>, List<Talk>, List<Talk>, List<Talk>, List<Timetable>> generate = null;

            generate = (slotNo, trackNo, slots, slot, slotTalks, talks) =>
            {
                if (slotNo == maxSlot)
                    return new List<Timetable>{slots};

                if (trackNo == maxTrack)
                    return generate(slotNo + 1, 0, new[]{slot}.Concat(slots).ToList(), new Talks(), talks, talks);

                return Selects(slotTalks).SelectMany(tuple =>
                {
                    var t = tuple.Item1;
                    var ts = tuple.Item2;
                    Talks clashesWithT;
                    if (!clashes.TryGetValue(t, out clashesWithT))
                    {
                        clashesWithT = new Talks();
                    }
                    var slotTalks2 = ts.Except(clashesWithT).ToList();
                    var talks2 = talks.Where(x => x != t).ToList();
                    return generate(slotNo, trackNo + 1, slots, new[]{t}.Concat(slot).ToList(), slotTalks2, talks2);
                }).ToList();
            };

            return generate(0, 0, new Timetable(), new Talks(), allTalks, allTalks);
        }

        private static ClashesMap BuildClashesMap(IEnumerable<Person> people)
        {
            return people
                .SelectMany(s => Selects(s.Talks))
                .ToLookup(
                    x => x.Item1,
                    x => x.Item2)
                .ToDictionary(
                    x => x.Key,
                    // ReSharper disable once PossibleMultipleEnumeration
                    x => x.SelectMany(y => y).Distinct().ToList());
        }

        private static IEnumerable<Tuple<T, IEnumerable<T>>> Selects<T>(IList<T> xs)
        {
            return xs.Select(x => Tuple.Create(x, xs.Except(new[]{x})));
        }
    }

    internal class Person
    {
        private readonly string _name;
        private readonly Talks _talks;

        public Person(string name, Talks talks)
        {
            _name = name;
            _talks = talks;
        }

        public string Name
        {
            get { return _name; }
        }

        public Talks Talks
        {
            get { return _talks; }
        }
    }
}
