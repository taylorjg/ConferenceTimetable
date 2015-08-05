using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck;

namespace CommonTypes
{
    public static class Utils
    {
        public static Gen<Tuple<IEnumerable<Person>, Talks>> GenTestData(
            int nslots,
            int ntracks,
            int ntalks,
            int npersons,
            int cPerS)
        {
            var totalTalks = nslots * ntracks;
            var talks = Enumerable.Range(1, totalTalks).Select(n => new Talk(n)).ToList();

            Func<int, Gen<IEnumerable<Person>>> mkPersons = null;

            mkPersons = n =>
            {
                if (n == 0) return Gen.Constant(Enumerable.Empty<Person>());

                return
                    from ts in FsCheckUtils.PickValues(cPerS, talks.Take(ntalks))
                    let name = string.Format("P{0}", n)
                    let person = new Person(name, new Talks(ts))
                    from rest in mkPersons(n - 1)
                    select rest.Concat(new[] { person });
            };

            return mkPersons(npersons).Select(ps => Tuple.Create(ps, new Talks(talks)));
        }

        public static Tuple<IEnumerable<Person>, Talks> GetHardCodedTestData()
        {
            var talks = Enumerable.Range(1, 12).Select(n => new Talk(n)).ToList();
            Func<IEnumerable<int>, Talks> getTalks = xs => new Talks(xs.Select(x => talks[x - 1]));
            var persons = new[]
            {
                new Person("P1", getTalks(new[] {11, 1, 8})),
                new Person("P2", getTalks(new[] {8, 4, 7})),
                new Person("P3", getTalks(new[] {7, 8, 4})),
                new Person("P4", getTalks(new[] {6, 9, 10})),
                new Person("P5", getTalks(new[] {10, 3, 7})),
                new Person("P6", getTalks(new[] {3, 11, 5})),
                new Person("P7", getTalks(new[] {8, 2, 5})),
                new Person("P8", getTalks(new[] {7, 2, 8})),
                new Person("P9", getTalks(new[] {4, 8, 6})),
                new Person("P10", getTalks(new[] {4, 8, 5}))
            }.AsEnumerable();

            return Tuple.Create(persons, new Talks(talks));
        }

        public static void DumpTestData(IEnumerable<Person> ps, Talks cs)
        {
            Console.WriteLine("cs: {0}", ListOfInts(cs.AsImmutableList().Select(t => t.Value)));
            foreach (var p in ps)
            {
                Console.WriteLine("p.Name: {0}; p.Talks: {1}", p.Name, ListOfInts(p.Talks.AsImmutableList().Select(t => t.Value)));
            }
        }

        public static void DumpTimetable(Timetable timetable)
        {
            foreach (var talks in timetable.AsImmutableList())
            {
                var line = ListOfInts(talks.AsImmutableList().Select(t => t.Value));
                Console.WriteLine(line);
            }
        }

        public static void DumpTimetables(IEnumerable<Timetable> timetables)
        {
            foreach (var timetable in timetables)
            {
                DumpTimetable(timetable);
                Console.WriteLine();
            }
        }

        private static string ListOfInts(IEnumerable<int> cs)
        {
            return string.Join(", ", cs.Select(x => Convert.ToString(x)));
        }
    }
}
