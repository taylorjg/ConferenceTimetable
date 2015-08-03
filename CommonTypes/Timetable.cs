using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CommonTypes
{
    public class Timetable
    {
        private readonly IImmutableList<Talks> _talks;

        public Timetable(IEnumerable<Talks> talks)
        {
            _talks = ImmutableList.CreateRange(talks);
        }

        public Timetable(params Talks[] talks)
            : this(talks.AsEnumerable())
        {
        }

        public Timetable(Talks talks, Timetable timetable)
            : this(new []{talks}.Concat(timetable.AsImmutableList()))
        {
        }

        public IImmutableList<Talks> AsImmutableList()
        {
            return _talks;
        }
    }
}
