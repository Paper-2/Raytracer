using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raytracer
{
    public class BVHNode : Hittable
    {
        Hittable Left;
        Hittable Right;

        // Thread-local Random to avoid cross-thread correlation and contention
        static readonly ThreadLocal<Random> rnd = new ThreadLocal<Random>(() =>
            new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));

        public BVHNode(HittableList list) : this(new List<Hittable>(list.Objects), 0, list.Objects.Count) { }
        public BVHNode(List<Hittable> objects, int start, int end)
        {
            if (start >= end)
                throw new ArgumentException($"BVHNode got empty span: start={start} end={end}");

            BoundingBox = AABB.Empty;
            for (int i = start; i < end; i++)
            {
                BoundingBox =  new AABB(BoundingBox, objects[i].BoundingBox);
            }

            int axis = BoundingBox.LongestAxis;
            int objectSpan = end - start;
            Comparison<Hittable> comparator;
            if (axis == 0)
                comparator = BoxCompareX;
            else if (axis == 1)
                comparator = BoxCompareY;
            else
                comparator = BoxCompareZ;

            if (objectSpan == 1)
            { 
                Left = Right = objects[start];
            }
            else if (objectSpan == 2)
            {
                Left = objects[start];
                Right = objects[start + 1];
            }
            else
            {                 
                objects.Sort(start, objectSpan, Comparer<Hittable>.Create(comparator));
                int mid = start + objectSpan / 2;
                Left = new BVHNode(objects, start, mid);
                Right = new BVHNode(objects, mid, end);
            }
        }

        public override bool Hit(Ray r, Interval rayT, ref HitRecord rec)
        {
            if (!BoundingBox.Hit(r, rayT)) { return false; }

            bool hitLeft = Left.Hit(r, rayT, ref rec);
            bool hitRight = Right.Hit(r, new Interval(rayT.Min, hitLeft ? rec.T : rayT.Max), ref rec);

            return hitLeft || hitRight;
        }
        static int BoxCompareX(Hittable a, Hittable b)
        {
            if (a.BoundingBox == null || b.BoundingBox == null)
                throw new InvalidOperationException("Hittable is missing a bounding box");
            return a.BoundingBox.X.Min.CompareTo(b.BoundingBox.X.Min);
        }
        static int BoxCompareY(Hittable a, Hittable b)
        {
            if (a.BoundingBox == null || b.BoundingBox == null)
                throw new InvalidOperationException("Hittable is missing a bounding box");
            return a.BoundingBox.Y.Min.CompareTo(b.BoundingBox.Y.Min);
        }
        static int BoxCompareZ(Hittable a, Hittable b)
        {
            if (a.BoundingBox == null || b.BoundingBox == null)
                throw new InvalidOperationException("Hittable is missing a bounding box");
            return a.BoundingBox.Z.Min.CompareTo(b.BoundingBox.Z.Min);
        }
    }
}
