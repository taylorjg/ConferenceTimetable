using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CommonTypes
{
    public class Talks
    {
        private readonly IImmutableList<Talk> _talks;

        public Talks(IEnumerable<Talk> talks)
        {
            _talks = ImmutableList.CreateRange(talks);
        }

        public Talks(params Talk[] talks)
            : this(talks.AsEnumerable())
        {
        }

        public Talks(Talk talk, Talks moreTalks)
            : this(new []{talk}.Concat(moreTalks.AsImmutableList()))
        {
        }

        public IImmutableList<Talk> AsImmutableList()
        {
            return _talks;
        }
    }
}
