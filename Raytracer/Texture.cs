using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;
using SharpNoise.Utilities.Imaging;

namespace Raytracer
{
    public abstract class Texture
    {
        public abstract Vec3 Value(double u, double v, Vec3 point);
    }

    public class SolidColor : Texture
    {
        public Vec3 Albedo { get; }
        public SolidColor(Vec3 albedo)
        {
            Albedo = albedo;
        }
        public SolidColor(double r, double g, double b) : this(new Vec3(r, g, b)) { }
        public override Vec3 Value(double u, double v, Vec3 point)
        {
            return Albedo;
        }
    }

    public class CheckerTexture : Texture
    {
        public Texture Odd { get; }
        public Texture Even { get; }
        public double InvScale { get; }
        public CheckerTexture(double scale, Texture even, Texture odd)
        {
            Odd = odd;
            Even = even;
            InvScale = 1.0 / scale;
        }
        public CheckerTexture(double scale, Vec3 c1, Vec3 c2) : this(scale, new SolidColor(c1), new SolidColor(c2)) { }
        public override Vec3 Value(double u, double v, Vec3 point)
        {
            int xInt = (int)Math.Floor(point.X * InvScale);
            int yInt = (int)Math.Floor(point.Y * InvScale);
            int zInt = (int)Math.Floor(point.Z * InvScale);

            bool isEven = (xInt + yInt + zInt) % 2 == 0;

            return isEven ? Even.Value(u, v, point) : Odd.Value(u, v, point);
        }
    }

    public class ImageTexture : Texture
    {
        public RtwImage Image { get; }
        public ImageTexture(string filename)
        {
            Image = new RtwImage(filename);
        }
        public override Vec3 Value(double u, double v, Vec3 point)
        {
            if (Image.Height <= 0 || Image.Width <= 0)
            {
                return new Vec3(0, 1, 1); // Cyan for error
            }

            u = new Interval(0, 1).Clamp(u);
            v = 1 - new Interval(0, 1).Clamp(v); // Flip V to image coordinates

            int i = (int)(u * Image.Width);
            int j = (int)(v * Image.Height);
            var pixel = Image.PixelData(i, j);

            double colorScale = 1.0 / 255.0;
            return new Vec3(colorScale * pixel.r, colorScale * pixel.g, colorScale * pixel.b);
        }
    }

    public class NoiseTexture : Texture
    {
        public Perlin Noise { get; } = new Perlin();
        public NoiseTexture(double scale)
        {
            Noise.Frequency = scale;
        }
        public override Vec3 Value(double u, double v, Vec3 point)
        {
            return new Vec3(1, 1, 1) * Noise.GetValue(point.X, point.Y, point.Z);
        }
    }
}
