using System;
using CommonTypes;

namespace Timetable2
{
    public class Partial : Tuple<int, int, Timetable, Talks, Talks, Talks>
    {
        public Partial(int slotNo, int trackNo, Timetable slots, Talks slot, Talks slotTalks, Talks talks)
            : base(slotNo, trackNo, slots, slot, slotTalks, talks)
        {
        }
    }
}
