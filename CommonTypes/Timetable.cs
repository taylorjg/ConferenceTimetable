using System.Collections.Immutable;
using System.Linq;

namespace CommonTypes
{
    public class Timetable
    {
        private readonly IImmutableList<Talks> _talks;

        public Timetable(IImmutableList<Talks> talks)
        {
            _talks = talks;
        }

        public Timetable(params Talks[] talks)
            : this(ImmutableList.CreateRange(talks))
        {
        }

        public Timetable(Talks talks, Timetable timetable)
            : this(new []{talks}.Concat(timetable.AsImmutableList()).ToArray())
        {
        }

        public Timetable() : this(ImmutableList<Talks>.Empty)
        {
        }

        public IImmutableList<Talks> AsImmutableList()
        {
            return _talks;
        }
    }
}
