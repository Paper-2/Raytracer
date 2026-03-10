using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Triangle : Hittable
    {
        public Vec3 V0 { get; set; }
        public Vec3 V1 { get; set; }
        public Vec3 V2 { get; set; }
        public Material Material { get; set; }
        public Vec3 Normal { get; set; }
        public Vec3 Edge1 { get; set; }
        public Vec3 Edge2 { get; set; }

        public Triangle(Vec3 v0, Vec3 v1, Vec3 v2, Material material)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            Material = material;

            Edge1 = V1 - V0;
            Edge2 = V2 - V0;
            Normal = Vec3.UnitVector(Vec3.Cross(Edge1, Edge2));

            SetBoundingBox();
        }

        void SetBoundingBox()
        {
            const double epsilon = 0.0001;
            Vec3 min = new Vec3(
                Math.Min(Math.Min(V0.X, V1.X), V2.X) - epsilon,
                Math.Min(Math.Min(V0.Y, V1.Y), V2.Y) - epsilon,
                Math.Min(Math.Min(V0.Z, V1.Z), V2.Z) - epsilon
            );
            Vec3 max = new Vec3(
                Math.Max(Math.Max(V0.X, V1.X), V2.X) + epsilon,
                Math.Max(Math.Max(V0.Y, V1.Y), V2.Y) + epsilon,
                Math.Max(Math.Max(V0.Z, V1.Z), V2.Z) + epsilon
            );
            BoundingBox = new AABB(min, max);
        }

        public override bool Hit(Ray r, Interval rayT, ref HitRecord rec)
        {
            // Möller-Trumbore intersection algorithm
            Vec3 h = Vec3.Cross(r.Direction, Edge2);
            double a = Vec3.Dot(Edge1, h);

            // Ray is parallel to the triangle
            if (Math.Abs(a) < 1e-8)
                return false;

            double f = 1.0 / a;
            Vec3 s = r.Origin - V0;
            double u = f * Vec3.Dot(s, h);

            // Intersection is outside the triangle
            if (u < 0.0 || u > 1.0)
                return false;

            Vec3 q = Vec3.Cross(s, Edge1);
            double v = f * Vec3.Dot(r.Direction, q);

            // Intersection is outside the triangle
            if (v < 0.0 || u + v > 1.0)
                return false;

            // Compute t to find the intersection point
            double t = f * Vec3.Dot(Edge2, q);

            if (!rayT.Contains(t))
                return false;

            // Ray intersection
            rec.T = t;
            rec.Point = r.At(t);
            rec.Material = Material;
            rec.SetFaceNormal(r, Normal);

            // Barycentric coordinates for UV mapping
            rec.U = u;
            rec.V = v;

            return true;
        }
    }
}
