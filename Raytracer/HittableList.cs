using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class HittableList: Hittable
    {
        public List<Hittable> Objects { get; }
        public HittableList()
        {
            Objects = new List<Hittable>();
        }
        public void Clear()
        {
            Objects.Clear();
            BoundingBox = null;
        }
        public void Add(Hittable obj)
        {
            Objects.Add(obj);
            if (obj.BoundingBox == null) return;
            BoundingBox = BoundingBox == null
                ? obj.BoundingBox
                : new AABB(BoundingBox, obj.BoundingBox);
        }
        //public void Add(HittableList list)
        //{
        //    foreach (var obj in list.Objects)
        //        Add(obj);
        //}

        public override bool Hit(Ray r, Interval rayT, ref HitRecord rec)
        {
            HitRecord tempRec = new HitRecord();
            bool hitAnything = false;
            double closestSoFar = rayT.Max;
            foreach (Hittable obj in Objects)
            {
                if (obj.Hit(r, new Interval(rayT.Min, closestSoFar), ref tempRec))
                {
                    hitAnything = true;
                    closestSoFar = tempRec.T;
                    rec = tempRec;
                }
            }
            return hitAnything;
        }
    }
}
