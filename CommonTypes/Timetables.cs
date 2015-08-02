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

        public IEnumerable<Timetable> AsEnumerable()
        {
            return _timetables;
        }
    }
}
