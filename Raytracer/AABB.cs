using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class AABB
    {
        public Interval X { get; set; }
        public Interval Y { get; set; }
        public Interval Z { get; set; }

        public AABB()
        {
            X = new Interval(0, 0);
            Y = new Interval(0, 0);
            Z = new Interval(0, 0);
        }
        public AABB(Interval x, Interval y, Interval z)
        {
            X = x;
            Y = y;
            Z = z;
            PadToMinimums();
        }
        public AABB(Vec3 a, Vec3 b)
        {
            // Treat the two points a and b as extrema for the bounding box, so we don't require a particular minimum/maximum coordinate order.
            X = (a.X <= b.X) ? new Interval(a.X, b.X) : new Interval(b.X, a.X);
            Y = (a.Y <= b.Y) ? new Interval(a.Y, b.Y) : new Interval(b.Y, a.Y);
            Z = (a.Z <= b.Z) ? new Interval(a.Z, b.Z) : new Interval(b.Z, a.Z);
            PadToMinimums();
        }
        public AABB(AABB box0, AABB box1)
        {
            X = new Interval(box0.X, box1.X);
            Y = new Interval(box0.Y, box1.Y);
            Z = new Interval(box0.Z, box1.Z);
        }

        public Interval AxisInterval(int axis)
        {
            switch (axis)
            {
                case 0: return X;
                case 1: return Y;
                case 2: return Z;
                default: throw new ArgumentException("Invalid axis");
            }
        }
        public bool Hit(Ray r, Interval rayT)
        {
            double tMin = rayT.Min;
            double tMax = rayT.Max;

            for (int axis = 0; axis < 3; axis++)
            {
                Interval ax = AxisInterval(axis);
                double adinv = 1.0 / r.Direction[axis];
                double t0 = (ax.Min - r.Origin[axis]) * adinv;
                double t1 = (ax.Max - r.Origin[axis]) * adinv;
                if (t0 < t1)
                {
                    if (t0 > tMin) tMin = t0;
                    if (t1 < tMax) tMax = t1;
                }
                else
                {
                    if (t1 > tMin) tMin = t1;
                    if (t0 < tMax) tMax = t0;
                }
                if (tMax <= tMin) return false;
            }
            return true;
        }

        public int LongestAxis
        {
            get
            {
                double xLength = X.Size();
                double yLength = Y.Size();
                double zLength = Z.Size();
                if (xLength > yLength && xLength > zLength)
                    return 0;
                else if (yLength > zLength)
                    return 1;
                else
                    return 2;
            }
        }

        void PadToMinimums()
        {
            double delta = 0.0001;
            if (X.Size() < delta) { X = X.Expand(delta); }
            if (Y.Size() < delta) { Y = Y.Expand(delta); }
            if (Z.Size() < delta) { Z = Z.Expand(delta); }
        }

        public static AABB operator +(AABB bBox, Vec3 offset)
        {
            return new AABB(bBox.X + offset.X, bBox.Y + offset.Y, bBox.Z + offset.Z);
        }
        public static AABB operator +(Vec3 offset, AABB bBox)
        {
            return bBox + offset;
        }

        public static AABB Empty
        {
            get
            {
                return new AABB(Interval.Empty(), Interval.Empty(), Interval.Empty());
            }
        }
        public static AABB Universe
        {
            get
            {
                return new AABB(Interval.Universe(), Interval.Universe(), Interval.Universe());
            }
        }
    }
}
