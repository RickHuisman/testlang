namespace testlang
{
    public class Value
    {
        public double Node;

        public Value(double value)
        {
            this.Node = value;
        }

        public override string ToString()
        {
            return this.Node.ToString();
        }
    }
}
