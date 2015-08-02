using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CommonTypes
{
    public class Talks
    {
        private readonly IImmutableList<Talk> _talks;

        public Talks(IImmutableList<Talk> talks)
        {
            _talks = talks;
        }

        public Talks(params Talk[] talks)
            : this(ImmutableList.CreateRange(talks))
        {
        }

        public Talks(IEnumerable<Talk> talks)
            : this(ImmutableList.CreateRange(talks))
        {
        }

        public Talks(Talk talk, Talks moreTalks)
            : this(new []{talk}.Concat(moreTalks.AsImmutableList()).ToArray())
        {
        }

        public Talks() : this(ImmutableList<Talk>.Empty)
        {
        }

        public IImmutableList<Talk> AsImmutableList()
        {
            return _talks;
        }
    }
}
