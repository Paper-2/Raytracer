using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raytracer
{
    public abstract class Material
    {
        public abstract bool Scatter(Ray rIn, HitRecord rec, out Vec3 attenuation, out Ray scattered);

        public virtual Vec3 Emitted(double u, double v, Vec3 p)
        {
            return new Vec3(0, 0, 0);
        }
    }

    public class Lambertian : Material
    {
        public Texture Tex { get; }
        public Lambertian(Texture tex)
        {             
            Tex = tex;
        }
        public Lambertian(Vec3 albedo) : this(new SolidColor(albedo)) { }
        public override bool Scatter(Ray rIn, HitRecord rec, out Vec3 attenuation, out Ray scattered)
        {
            Vec3 scatterDirection = rec.Normal + Vec3.RandomUnitVector();

            // Catch degenerate scatter direction
            if (scatterDirection.NearZero())
            {
                scatterDirection = rec.Normal;
            }
            scattered = new Ray(rec.Point, scatterDirection, rIn.Time);
            attenuation = Tex.Value(rec.U, rec.V, rec.Point);
            return true;
        }
    }

    public class Metal : Material
    {
        public Vec3 Albedo { get; }
        public double Fuzz { get; }
        public Metal(Vec3 albedo, double fuzz)
        {
            Albedo = albedo;
            Fuzz = Math.Max(0, Math.Min(1, fuzz));
        }
        public override bool Scatter(Ray rIn, HitRecord rec, out Vec3 attenuation, out Ray scattered)
        {
            Vec3 reflected = Vec3.Reflect(rIn.Direction, rec.Normal);
            reflected = Vec3.UnitVector(reflected) + Fuzz * Vec3.RandomUnitVector();
            scattered = new Ray(rec.Point, reflected, rIn.Time);
            attenuation = Albedo;
            return Vec3.Dot(scattered.Direction, rec.Normal) > 0;
        }
    }

    public class Dielectric : Material
    {
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

        public double RefractionIndex { get; }
        public Dielectric(double refractionIndex)
        {
            RefractionIndex = refractionIndex;
        }
        public override bool Scatter(Ray rIn, HitRecord rec, out Vec3 attenuation, out Ray scattered)
        {
            attenuation = new Vec3(1.0, 1.0, 1.0);
            double ri = rec.FrontFace ? (1.0 / RefractionIndex) : RefractionIndex;

            Vec3 unitDirection = Vec3.UnitVector(rIn.Direction);
            double cosTheta = Math.Min(Vec3.Dot(-unitDirection, rec.Normal), 1.0);
            double sinTheta = Math.Sqrt(1.0 - cosTheta * cosTheta);

            bool cannotRefract = ri * sinTheta > 1.0;
            Vec3 direction;
            if (cannotRefract || Reflectance(cosTheta, ri) > s_rnd.Value.NextDouble())
            {
                direction = Vec3.Reflect(unitDirection, rec.Normal);
            }
            else
            {
                direction = Vec3.Refract(unitDirection, rec.Normal, ri);
            }

            scattered = new Ray(rec.Point, direction, rIn.Time);
            return true;
        }
        private static double Reflectance(double cosine, double refIdx)
        {
            // Use Schlick's approximation for reflectance.
            double r0 = (1 - refIdx) / (1 + refIdx);
            r0 = r0 * r0;
            return r0 + (1 - r0) * Math.Pow((1 - cosine), 5);
        }
    }

    public class DiffuseLight : Material
    {
        public Texture Texture { get; }
        public DiffuseLight(Texture texture)
        {
            Texture = texture;
        }
        public DiffuseLight(Vec3 emit) : this(new SolidColor(emit)) { }
        public override bool Scatter(Ray rIn, HitRecord rec, out Vec3 attenuation, out Ray scattered)
        {
            attenuation = new Vec3(0, 0, 0);
            scattered = null;
            return false;
        }
        public override Vec3 Emitted(double u, double v, Vec3 p)
        {
            return Texture.Value(u, v, p);
        }
    }
}
