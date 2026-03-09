using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    public class Ray
    {
        public Vec3 Origin { get; }
        public Vec3 Direction { get; }
        public double Time { get; }

        public Ray() { }
        public Ray(Vec3 origin, Vec3 direction)
        {
            Origin = origin;
            Direction = direction;
            Time = 0;
        }
        public Ray(Vec3 origin, Vec3 direction, double time)
        {
            Origin = origin;
            Direction = direction;
            Time = time;
        }

        public Vec3 At(double t)
        {
            return Origin + Direction * t;
        }
    }
}
