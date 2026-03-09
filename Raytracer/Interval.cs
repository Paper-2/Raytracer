using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Interval
    {
        public double Min { get; set; }
        public double Max { get; set; }

        public Interval()
        {
            Min = double.MaxValue;
            Max = double.MinValue;
        }
        public Interval(double min, double max)
        {
            Min = min;
            Max = max;
        }
        public Interval(Interval a, Interval b)
        {
            Min = Math.Min(a.Min, b.Min);
            Max = Math.Max(a.Max, b.Max);
        }
        public static Interval Empty()
        {
            return new Interval();
        }
        public static Interval Universe()
        {
            return new Interval(double.MinValue, double.MaxValue);
        }

        public double Size()
        {
            return Max - Min;
        }
        public bool Contains(double x)
        {
            return Min <= x && x <= Max;
        }
        public bool Surrounds(double x)
        {
            return Min < x && x < Max;
        }
        public double Clamp(double x)
        {
            if (x < Min) { return Min; }
            if (x > Max) { return Max; }
            return x;
        }
        public Interval Expand(double delta)
        {
            double padding = delta / 2.0;
            return new Interval(Min - padding, Max + padding);
        }

        public static Interval operator +(Interval interval, double displacement)
        {
            return new Interval(interval.Min + displacement, interval.Max + displacement);
        }
        public static Interval operator +(double displacement, Interval interval)
        {
            return interval + displacement;
        }
    }
}
