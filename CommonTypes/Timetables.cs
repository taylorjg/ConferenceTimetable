using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CommonTypes
{
    public class Timetables
    {
        private readonly IImmutableList<Timetable> _timetables;

        public Timetables(IEnumerable<Timetable> timetables)
        {
            _timetables = ImmutableList.CreateRange(timetables);
        }

        public Timetables(params Timetable[] timetables)
            : this(timetables.AsEnumerable())
        {
        }

        public IImmutableList<Timetable> AsImmutableList()
        {
            return _timetables;
        }
    }
}
