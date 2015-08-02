using System.Collections.Immutable;

namespace CommonTypes
{
    public class Person
    {
        private readonly string _name;
        private readonly Talks _talks;

        public Person(string name, Talks talks)
        {
            _name = name;
            _talks = talks;
        }

        public Person(string name, params Talk[] talks)
            : this(name, new Talks(talks))
        {
        }

        public string Name
        {
            get { return _name; }
        }

        public Talks Talks
        {
            get { return _talks; }
        }
    }
}
