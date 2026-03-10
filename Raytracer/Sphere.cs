using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Sphere : Hittable
    {
        public Ray Center { get; }
        public double Radius { get; }
        public Material Material { get; }

        // Stationary sphere constructor
        public Sphere(Vec3 center, double radius, Material material)
        {
            Center = new Ray(center, new Vec3(0, 0, 0));
            Radius = Math.Max(0, radius);
            Material = material;
            Vec3 rvec = new Vec3(Radius, Radius, Radius);
            BoundingBox = new AABB(center - rvec, center + rvec);
        }
        // Moving sphere constructor
        public Sphere(Vec3 center1, Vec3 center2, double radius, Material material)
        {
            Center = new Ray(center1, center2 - center1);
            Radius = Math.Max(0, radius);
            Material = material;
            Vec3 rvec = new Vec3(Radius, Radius, Radius);
            AABB box1 = new AABB(Center.At(0) - rvec, Center.At(0) + rvec);
            AABB box2 = new AABB(Center.At(1) - rvec, Center.At(1) + rvec);
            BoundingBox = new AABB(box1, box2);
        }

        public override bool Hit(Ray r, Interval rayT, ref HitRecord rec)
        {
            Vec3 currentCenter = Center.At(r.Time);
            Vec3 oc = currentCenter - r.Origin;
            double a = r.Direction.LengthSquared();
            double h = Vec3.Dot(r.Direction, oc);
            double c = oc.LengthSquared() - Radius * Radius;

            double discriminant = h * h - a * c;
            if (discriminant < 0)
            {
                return false;
            }

            double sqrtDiscriminant = Math.Sqrt(discriminant);

            double root = (h - sqrtDiscriminant) / a;
            if (!rayT.Surrounds(root))
            {
                root = (h + sqrtDiscriminant) / a;
                if (!rayT.Surrounds(root))
                {
                    return false;
                }
            }

            rec.T = root;
            rec.Point = r.At(rec.T);
            Vec3 outwardNormal = (rec.Point - currentCenter) / Radius;
            rec.SetFaceNormal(r, outwardNormal);
            rec.Material = Material;
            getSphereUV((rec.Point - currentCenter) / Radius, out double recU, out double recV);
            rec.U = recU;
            rec.V = recV;

            return true;
        }

        void getSphereUV(Vec3 p, out double u, out double v)
        {
            // p: a given point on the sphere of radius one, centered at the origin.
            // u: returned value [0,1] of angle around the Y axis from X=-1.
            // v: returned value [0,1] of angle from Y=-1 to Y=+1.
            //     <1 0 0> yields <0.50 0.50>       <-1 0 0> yields <0.00 0.50>
            //     <0 1 0> yields <0.50 1.00>       < 0 -1 0> yields <0.50 0.00>
            //     <0 0 1> yields <0.25 0.50>       < 0 0 -1> yields <0.75 0.50>
            double theta = Math.Acos(-p.Y);
            double phi = Math.Atan2(-p.Z, p.X) + Math.PI;
            u = phi / (2 * Math.PI);
            v = theta / Math.PI;
        }
    }
}
