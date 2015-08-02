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
