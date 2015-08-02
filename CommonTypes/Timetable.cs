using System.Collections.Generic;
using System.Collections.Immutable;

namespace CommonTypes
{
    public class Timetable
    {
        private readonly IImmutableList<Talks> _timetable;

        public Timetable(IImmutableList<Talks> timetable)
        {
            _timetable = timetable;
        }

        public IEnumerable<Talks> AsEnumerable()
        {
            return _timetable;
        }
    }
}
