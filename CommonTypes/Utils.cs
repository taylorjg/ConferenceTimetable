using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static void RunWithStats(Action action)
        {
            var process1 = Process.GetCurrentProcess();
            var beforeWorkingSet64 = process1.WorkingSet64;
            var beforePeakWorkingSet64 = process1.PeakWorkingSet64;
            var beforeVirtualMemorySize64 = process1.VirtualMemorySize64;
            var beforePeakVirtualMemorySize64 = process1.PeakVirtualMemorySize64;
            Console.WriteLine("WorkingSet64: {0:N0}", beforeWorkingSet64);
            Console.WriteLine("PeakWorkingSet64: {0:N0}", beforePeakWorkingSet64);
            Console.WriteLine("VirtualMemorySize64: {0:N0}", beforeVirtualMemorySize64);
            Console.WriteLine("PeakVirtualMemorySize64: {0:N0}", beforePeakVirtualMemorySize64);

            var stopwatch = Stopwatch.StartNew();

            action();

            stopwatch.Stop();
            Console.WriteLine("stopwatch.Elapsed: {0}", stopwatch.Elapsed);

            var process2 = Process.GetCurrentProcess();
            var afterWorkingSet64 = process2.WorkingSet64;
            var afterPeakWorkingSet64 = process2.PeakWorkingSet64;
            var afterVirtualMemorySize64 = process2.VirtualMemorySize64;
            var afterPeakVirtualMemorySize64 = process2.PeakVirtualMemorySize64;
            Console.WriteLine("WorkingSet64: {0:N0} ({1:N0})", afterWorkingSet64, afterWorkingSet64 - beforeWorkingSet64);
            Console.WriteLine("PeakWorkingSet64: {0:N0} ({1:N0})", afterPeakWorkingSet64, afterPeakWorkingSet64 - beforePeakWorkingSet64);
            Console.WriteLine("VirtualMemorySize64: {0:N0} ({1:N0})", afterVirtualMemorySize64, afterVirtualMemorySize64 - beforeVirtualMemorySize64);
            Console.WriteLine("PeakVirtualMemorySize64: {0:N0} ({1:N0})", afterPeakVirtualMemorySize64, afterPeakVirtualMemorySize64 - beforePeakVirtualMemorySize64);
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
