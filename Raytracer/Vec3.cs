using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Vec3
    {
        double[] e = new double[3];

        public Vec3()
        {
            e = new double[] { 0, 0, 0 };
        }
        public Vec3(double x, double y, double z)
        {
            e = new double[] { x, y, z };
        }

        public double X { get { return e[0]; } }
        public double Y { get { return e[1]; } }
        public double Z { get { return e[2]; } }

        public static Vec3 operator -(Vec3 a)
        {
            return new Vec3(-a.X, -a.Y, -a.Z);
        }
        public double this[int index]
        {
            get { return e[index]; }
            set { e[index] = value; }
        }
        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static Vec3 operator -(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static Vec3 operator *(Vec3 a, double scalar)
        {
            return new Vec3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }
        public static Vec3 operator *(double scalar, Vec3 a)
        {
            return a * scalar;
        }
        public static Vec3 operator *(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        public static Vec3 operator /(Vec3 a, double scalar)
        {
            return new Vec3(a.X / scalar, a.Y / scalar, a.Z / scalar);
        }
        public double Length()
        {
            return Math.Sqrt(LengthSquared());
        }
        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }
        public static double Dot(Vec3 a, Vec3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }
        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            return new Vec3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }
        public static Vec3 UnitVector(Vec3 a)
        {
            return a / a.Length();
        }
        public bool NearZero()
        {
            // Return true if the vector is close to zero in all dimensions.
            const double s = 1e-8;
            return (Math.Abs(X) < s) && (Math.Abs(Y) < s) && (Math.Abs(Z) < s);
        }
        public static Vec3 Reflect(Vec3 v, Vec3 n)
        {
            return v - 2 * Dot(v, n) * n;
        }
        public static Vec3 Refract(Vec3 uv, Vec3 n, double etaiOverEtat)
        {
            double cosTheta = Math.Min(Dot(-uv, n), 1.0);
            Vec3 rOutPerp = etaiOverEtat * (uv + cosTheta * n);
            Vec3 rOutParallel = -Math.Sqrt(Math.Abs(1.0 - rOutPerp.LengthSquared())) * n;
            return rOutPerp + rOutParallel;
        }

        // AI Code
        // Thread-local PRNG to avoid reseeding patterns that cause visible banding.
        // Seeded safely so each thread gets a different Random instance.
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

        public static Vec3 Random()
        {
            var rnd = s_rnd.Value;
            return new Vec3(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble());
        }
        public static Vec3 Random(double min, double max)
        {
            var rnd = s_rnd.Value;
            return new Vec3(rnd.NextDouble() * (max - min) + min, rnd.NextDouble() * (max - min) + min, rnd.NextDouble() * (max - min) + min);
        }
        public static Vec3 RandomUnitVector()
        {
            while (true)
            {
                Vec3 p = Vec3.Random(-1, 1);
                double lengthSquared = p.LengthSquared();
                if (1e-160 < lengthSquared && lengthSquared <= 1)
                {
                    return p / Math.Sqrt(lengthSquared);
                }
            }
        }
        public static Vec3 RandomOnHemisphere(Vec3 normal)
        {
            Vec3 onUnitSphere = Vec3.RandomUnitVector();
            if (Vec3.Dot(onUnitSphere, normal) > 0.0) // In the same hemisphere as the normal
            {
                return onUnitSphere;
            }
            else
            {
                return -onUnitSphere;
            }
        }
        public static Vec3 RandomInUnitDisk()
        {
            while (true)
            {
                Vec3 p = new Vec3(s_rnd.Value.NextDouble() * 2 - 1, s_rnd.Value.NextDouble() * 2 - 1, 0);
                if (p.LengthSquared() < 1)
                {
                    return p;
                }
            }
        }

        double LinearToGamma(double linearComponent)
        {
            if (linearComponent > 0) { return Math.Sqrt(linearComponent); }
            return 0;
        }
        public string ToColor()
        {
            Interval intensity = new Interval(0, 0.999);
            int r = (int)(255.999 * intensity.Clamp(LinearToGamma(X)));
            int g = (int)(255.999 * intensity.Clamp(LinearToGamma(Y)));
            int b = (int)(255.999 * intensity.Clamp(LinearToGamma(Z)));
            return $"{r} {g} {b}";
        }
        public override string ToString()
        {
            return $"{X} {Y} {Z}";
        }
    }
}
