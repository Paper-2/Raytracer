using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public abstract class Hittable
    {
        public abstract bool Hit(Ray r, Interval rayT, out HitRecord rec);

        public virtual AABB BoundingBox { get; set; }
    }

    public class HitRecord
    {
        public Vec3 Point { get; set; }
        public Vec3 Normal { get; set; }
        public Material Material { get; set; }
        public double T { get; set; }
        public double U { get; set; }
        public double V { get; set; }
        public bool FrontFace { get; set; }

        public void SetFaceNormal(Ray r, Vec3 outwardNormal)
        {
            // Sets the hit record normal vector.
            // NOTE: the parameter `outward_normal` is assumed to have unit length.
            FrontFace = Vec3.Dot(r.Direction, outwardNormal) < 0;
            Normal = FrontFace ? outwardNormal : -outwardNormal;
        }

        public HitRecord(Vec3 point, double t, Ray r, Vec3 outwardNormal, Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material), "HitRecord created with null material!");
            Point = point;
            T = t;
            SetFaceNormal(r, outwardNormal);
            Material = material;
        }
    }

    public class Translate : Hittable
    {
        public Hittable Object { get; }
        public Vec3 Offset { get; }
        public Translate(Hittable obj, Vec3 offset)
        {
            Object = obj;
            Offset = offset;
            BoundingBox = BoundingBox + Offset;
        }
        public override bool Hit(Ray r, Interval rayT, out HitRecord rec)
        {
            Ray movedRay = new Ray(r.Origin - Offset, r.Direction, r.Time);
            if (!Object.Hit(movedRay, rayT, out rec))
                return false;
            rec.Point += Offset;
            return true;
        }
    }

    public class RotateY : Hittable
    {
        public Hittable Object { get; }
        public double SinTheta { get; }
        public double CosTheta { get; }
        public RotateY(Hittable obj, double angle)
        {
            Object = obj;
            double radians = (Math.PI / 180) * angle;
            SinTheta = Math.Sin(radians);
            CosTheta = Math.Cos(radians);
            BoundingBox = RotateBBox(Object.BoundingBox);
        }
        AABB RotateBBox(AABB bbox)
        {
            Vec3 min = new Vec3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            Vec3 max = new Vec3(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                    {
                        double x = i * bbox.X.Max + (1 - i) * bbox.X.Min;
                        double y = j * bbox.Y.Max + (1 - j) * bbox.Y.Min;
                        double z = k * bbox.Z.Max + (1 - k) * bbox.Z.Min;
                        double newX = CosTheta * x + SinTheta * z;
                        double newZ = -SinTheta * x + CosTheta * z;
                        Vec3 tester = new Vec3(newX, y, newZ);
                        for (int c = 0; c < 3; c++)
                        {
                            min[c] = Math.Min(min[c], tester[c]);
                            max[c] = Math.Max(max[c], tester[c]);
                        }
                    }
            return new AABB(min, max);
        }
        public override bool Hit(Ray r, Interval rayT, out HitRecord rec)
        {
            rec = null;
            Vec3 origin = new Vec3(CosTheta * r.Origin.X - SinTheta * r.Origin.Z,
                                   r.Origin.Y,
                                   SinTheta * r.Origin.X + CosTheta * r.Origin.Z);
            Vec3 direction = new Vec3(CosTheta * r.Direction.X - SinTheta * r.Direction.Z,
                                      r.Direction.Y,
                                      SinTheta * r.Direction.X + CosTheta * r.Direction.Z);
            Ray rotatedRay = new Ray(origin, direction, r.Time);

            if (!Object.Hit(rotatedRay, rayT, out rec))
                return false;

            Vec3 recP = new Vec3(CosTheta * rec.Point.X + SinTheta * rec.Point.Z,
                                 rec.Point.Y,
                                 -SinTheta * rec.Point.X + CosTheta * rec.Point.Z);
            Vec3 recNormal = new Vec3(CosTheta * rec.Normal.X + SinTheta * rec.Normal.Z,
                                        rec.Normal.Y,
                                        -SinTheta * rec.Normal.X + CosTheta * rec.Normal.Z);
            rec = new HitRecord(recP, rec.T, rotatedRay, recNormal, rec.Material); // TODO: HOEH?
            return true;
        }
    }
}