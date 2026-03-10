using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raytracer
{
    public class ConstantMedium : Hittable
    {
        Hittable Boundary;
        double NegInvDensity;
        Material PhaseFunction;

        public ConstantMedium(Hittable boundary, double density, Vec3 albedo)
        {
            Boundary = boundary;
            NegInvDensity = -1 / density;
            PhaseFunction = new Isotropic(albedo);
            BoundingBox = Boundary.BoundingBox;
        }
        public ConstantMedium(Hittable boundary, double negInvDensity, Texture texture)
        {
            Boundary = boundary;
            NegInvDensity = negInvDensity;
            PhaseFunction = new Isotropic(texture);
            BoundingBox = Boundary.BoundingBox;
        }

        // AI
        // Thread-local PRNG to avoid reseeding and correlated sequences.
        private static readonly object s_globalRandLock = new object();
        private static int s_globalSeed = Environment.TickCount;
        private static readonly ThreadLocal<Random> s_rnd = new ThreadLocal<Random>(() =>
        {
            int seed;
            lock (s_globalRandLock)
            {
                seed = ++s_globalSeed;
            }
            return new Random(seed);
        });
        public override bool Hit(Ray r, Interval rayT, ref HitRecord rec)
        {
            HitRecord rec1 = new HitRecord();
            HitRecord rec2 = new HitRecord();

            if (!Boundary.Hit(r, Interval.Universe(), ref rec1))
                return false;
            if (!Boundary.Hit(r, new Interval(rec1.T + 1e-4, double.PositiveInfinity), ref rec2))
                return false;

            if (rec1.T < rayT.Min) rec1.T = rayT.Min;
            if (rec2.T > rayT.Max) rec2.T = rayT.Max;

            if (rec1.T >= rec2.T)
                return false;

            if (rec1.T < 0)
                rec1.T = 0;

            double rayLength = r.Direction.Length();
            double distanceInsideBoundary = (rec2.T - rec1.T) * rayLength;
            double hitDistance = NegInvDensity * Math.Log(s_rnd.Value.NextDouble());

            if (hitDistance > distanceInsideBoundary)
                return false;

            rec.T = rec1.T + hitDistance / rayLength;
            rec.Point = r.At(rec.T);
            rec.Normal = new Vec3(1, 0, 0); // arbitrary
            rec.FrontFace = true; // also arbitrary
            rec.Material = PhaseFunction;

            return true;
        }
    }
}
