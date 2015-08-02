using System.Collections.Generic;
using System.Collections.Immutable;

namespace CommonTypes
{
    public class Talks
    {
        private readonly IImmutableList<Talk> _talks;

        public Talks(IImmutableList<Talk> talks)
        {
            _talks = talks;
        }

        public IEnumerable<Talk> AsEnumerable()
        {
            return _talks;
        }
    }
}
