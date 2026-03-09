using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Quad : Hittable
    {
        public Vec3 Q { get; set; }
        public Vec3 U { get; set; }
        public Vec3 V { get; set; }
        public Vec3 W { get; set; }
        public Material Material { get; set; }
        public Vec3 Normal { get; set; }
        public double D { get; set; }

        public Quad(Vec3 q, Vec3 u, Vec3 v, Material material)
        {
            Q = q;
            U = u;
            V = v;
            Material = material;

            Vec3 n = Vec3.Cross(U, V);
            Normal = Vec3.UnitVector(n);
            D = Vec3.Dot(Normal, Q);
            W = n / Vec3.Dot(n, n);
            SetBoundingBox();
        }

        void SetBoundingBox()
        {
            AABB bBox1 = new AABB(Q, Q + U + V);
            AABB bBox2 = new AABB(Q + U, Q + V);
            BoundingBox = new AABB(bBox1, bBox2);
        }

        public override bool Hit(Ray r, Interval rayT, out HitRecord rec)
        {
            rec = null;
            double denom = Vec3.Dot(Normal, r.Direction);

            // No hit if ray is parallel to the plane of the quad
            if (Math.Abs(denom) < 1e-8)
                return false;

            // Return false if the hit is outside the ray's t range
            double t = (D - Vec3.Dot(Normal, r.Origin)) / denom;
            if (!rayT.Contains(t))
                return false;

            // Determine if the hit point lies within the planar shape using its plane coordinates.
            Vec3 intersection = r.At(t);
            Vec3 planarHitVector = intersection - Q;
            double alpha = Vec3.Dot(W, Vec3.Cross(planarHitVector, V));
            double beta = Vec3.Dot(W, Vec3.Cross(U, planarHitVector));

            if (!IsInterior(alpha, beta, out double recU, out double recV))
                return false;

            // Ray hits the 2D shape; set the rest of the hit record and return true.
            rec = new HitRecord(intersection, t, r, Normal, Material);
            rec.U = recU;
            rec.V = recV;
            return true;
        }

        public virtual bool IsInterior(double alpha, double beta, out double recU, out double recV)
        {
            recU = recV = 0;
            Interval unitInterval = new Interval(0, 1);
            if (!unitInterval.Contains(alpha) || !unitInterval.Contains(beta))
            {
                return false;
            }

            recU = alpha;
            recV = beta;
            return true;
        }

        public static HittableList Box(Vec3 a, Vec3 b, Material material)
        {
            // Returns the 3D box (six sides) that contains the two opposite vertices a & b.
            HittableList sides = new HittableList();

            Vec3 min = new Vec3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
            Vec3 max = new Vec3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));

            Vec3 dx = new Vec3(max.X - min.X, 0, 0);
            Vec3 dy = new Vec3(0, max.Y - min.Y, 0);
            Vec3 dz = new Vec3(0, 0, max.Z - min.Z);

            sides.Add(new Quad(new Vec3(min.X, min.Y, max.Z),  dx,  dy, material)); // Front
            sides.Add(new Quad(new Vec3(max.X, min.Y, max.Z), -dz,  dy, material)); // Right
            sides.Add(new Quad(new Vec3(max.X, min.Y, min.Z), -dx,  dy, material)); // Back
            sides.Add(new Quad(new Vec3(min.X, min.Y, min.Z),  dz,  dy, material)); // Left
            sides.Add(new Quad(new Vec3(min.X, max.Y, max.Z),  dx, -dz, material)); // Top
            sides.Add(new Quad(new Vec3(min.X, min.Y, min.Z),  dx,  dz, material)); // Bottom
            
            return sides;
        }
    }
}