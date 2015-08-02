using System.Collections.Generic;
using System.Collections.Immutable;

namespace CommonTypes
{
    public class Timetables
    {
        private readonly IImmutableList<Timetable> _timetables;

        public Timetables(IImmutableList<Timetable> timetables)
        {
            _timetables = timetables;
        }

        public Timetables(params Timetable[] timetables)
            : this(ImmutableList.CreateRange(timetables))
        {
        }

        public Timetables(IEnumerable<Timetable> timetables)
            : this(ImmutableList.CreateRange(timetables))
        {
        }

        public Timetables()
            : this(ImmutableList<Timetable>.Empty)
        {
        }

        public IImmutableList<Timetable> AsImmutableList()
        {
            return _timetables;
        }
    }
}
