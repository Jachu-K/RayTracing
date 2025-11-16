namespace RayTracing
{
    public class Interval
    {
        public double Min { get; }
        public double Max { get; }

        public static readonly Interval Empty = new Interval(double.PositiveInfinity, double.NegativeInfinity);
        public static readonly Interval Universe = new Interval(double.NegativeInfinity, double.PositiveInfinity);

        public Interval(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public double Size => Max - Min;
        public bool Contains(double x) => Min <= x && x <= Max;
        public bool Surrounds(double x) => Min < x && x < Max;
        public double Clamp(double x)
        {
            if (x < Min) return Min;
            if (x > Max) return Max;
            return x;
        }
    }
}