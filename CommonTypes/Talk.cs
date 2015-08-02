namespace CommonTypes
{
    public class Talk
    {
        private readonly int _value;

        public Talk(int value)
        {
            _value = value;
        }

        public int Value
        {
            get { return _value; }
        }
    }
}
