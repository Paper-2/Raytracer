using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Raytracer
{
    public class Camera
    {
        public double AspectRatio { get; set; }  // Ratio of image width over height
        public int ImageWidth { get; set; }      // Rendered image width in pixel count
        public int SamplesPerPixel { get; set; } // Count of random samples for each pixel
        public int MaxDepth { get; set; }        // Maximum number of ray bounces into scene
        public Vec3 Background { get; set; } = new Vec3(0, 0, 0); // Scene background color

        public double VFov { get; set; }         // Vertical view angle (field of view)
        public Vec3 LookFrom { get; set; } = new Vec3(0, 0, 0); // Point camera is looking from
        public Vec3 LookAt { get; set; } = new Vec3(0, 0, -1);  // Point camera is looking at
        public Vec3 VUp { get; set; } = new Vec3(0, 1, 0);      // Camera-relative "up" direction

        public double DefocusAngle { get; set; } // Variation angle of rays through each pixel
        public double FocusDist { get; set; }    // Distance from camera lookfrom point to plane of perfect focus

        readonly ThreadLocal<Random> rnd = new ThreadLocal<Random>(() =>
            new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));

        public Camera(double aspectRatio = 1.0, int imageWidth = 100, int samplesPerPixel = 10,
            int maxDepth = 10, Vec3 background = null, double vFov = 90, Vec3 lookFrom = null, Vec3 lookAt = null, Vec3 vUp = null, double defocusAngle = 0, double focusDist = 10)
        {
            AspectRatio = aspectRatio;
            ImageWidth = imageWidth;
            SamplesPerPixel = samplesPerPixel;
            MaxDepth = maxDepth;
            if (background != null) { Background = background; }
            VFov = vFov;
            if (lookFrom != null) { LookFrom = lookFrom; }
            if (lookAt != null) { LookAt = lookAt; }
            if (vUp != null) { VUp = vUp; }
            DefocusAngle = defocusAngle;
            FocusDist = focusDist;
        }

        int imageHeight;            // Rendered image height
        double pixelSamplesScale;   // Color scale factor for a sum of pixel samples
        Vec3 center;                // Camera center
        Vec3 pixel00Loc;            // Location of pixel 0, 0
        Vec3 pixelDeltaU;           // Offset to pixel to the right
        Vec3 pixelDeltaV;           // Offset to pixel below
        Vec3 w, u, v;               // Camera frame basis vectors
        Vec3 defocusDiskU;          // Defocus disk horizontal radius
        Vec3 defocusDiskV;          // Defocus disk vertical radius

        Vec3 SampleSquare()
        {
            // Returns the vector to a random point in the [-.5,-.5]-[+.5,+.5] unit square.
            var r = rnd.Value;
            return new Vec3(r.NextDouble() - 0.5, r.NextDouble() - 0.5, 0);
        }
        Vec3 DefocusDiskSample()
        {
            // Returns a random point in the camera defocus disk.
            Vec3 p = Vec3.RandomInUnitDisk();
            return center + p.X * defocusDiskU + p.Y * defocusDiskV;
        }
        Ray GetRay(int i, int j)
        {
            // Construct a camera ray originating from the defocus disk and directed at a randomly
            // sampled point around the pixel location i, j.
            var r = rnd.Value;

            Vec3 offset = SampleSquare();
            Vec3 pixelSample = pixel00Loc + (i + offset.X) * pixelDeltaU + (j + offset.Y) * pixelDeltaV;

            Vec3 rayOrigin = (DefocusAngle <= 0) ? center : DefocusDiskSample();
            Vec3 rayDirection = pixelSample - rayOrigin;
            double rayTime = r.NextDouble();

            return new Ray(rayOrigin, rayDirection, rayTime);
        }

        Vec3 RayColor(Ray ray, int depth, Hittable world)
        {
            if (depth <= 0) { return new Vec3(0, 0, 0); }

            HitRecord rec = new HitRecord();
            if (!world.Hit(ray, new Interval(0.001, double.PositiveInfinity), ref rec))
            {
                return Background;
            }

            Ray scattered = null;
            Vec3 attenuation = null;
            Vec3 colorFromEmission = rec.Material.Emitted(rec.U, rec.V, rec.Point);

            if (!rec.Material.Scatter(ray, rec, out attenuation, out scattered))
            {
                return colorFromEmission;
            }

            Vec3 colorFromScatter = attenuation * RayColor(scattered, depth -1, world);
            return colorFromEmission + colorFromScatter;
        }

        void Initialize()
        {
            imageHeight = (int)(ImageWidth / AspectRatio);
            imageHeight = imageHeight < 1 ? 1 : imageHeight;

            pixelSamplesScale = 1.0 / SamplesPerPixel;

            center = LookFrom;

            // Determine viewport dimensions.
            double theta = VFov * Math.PI / 180.0;
            double h = Math.Tan(theta / 2);
            double viewportHeight = 2.0 * h * FocusDist;
            double viewportWidth = viewportHeight * ((double)ImageWidth / imageHeight);

            // Calculate the u,v,w unit basis vectors for the camera coordinate frame.
            w = Vec3.UnitVector(LookFrom - LookAt);
            u = Vec3.UnitVector(Vec3.Cross(VUp, w));
            v = Vec3.Cross(w, u);

            // Calculate the vectors across the horizontal and down the vertical viewport edges.
            Vec3 viewportU = viewportWidth * u;
            Vec3 viewportV = viewportHeight * -v;

            // Calculate the horizontal and vertical delta vectors from pixel to pixel.
            pixelDeltaU = viewportU / ImageWidth;
            pixelDeltaV = viewportV / imageHeight;

            // Calculate the location of the upper left pixel.
            Vec3 viewportUpperLeft = center - FocusDist * w - 0.5 * viewportU - 0.5 * viewportV;
            pixel00Loc = viewportUpperLeft + 0.5 * (pixelDeltaU + pixelDeltaV);

            double defocusRadius = FocusDist * Math.Tan(DefocusAngle * Math.PI / 180.0 / 2);
            defocusDiskU = defocusRadius * u;
            defocusDiskV = defocusRadius * v;
        }

        public string Render(Hittable world)
        {
            Initialize();

            string[] image = new string[ImageWidth * imageHeight];
            int linesProcessed = 0;
            Parallel.For(0, imageHeight, j =>
             {
                 Parallel.For(0, ImageWidth, i =>
                 {
                     Vec3 pixelColor = new Vec3(0, 0, 0);
                     for (int sample = 0; sample < SamplesPerPixel; sample++)
                     {
                         Ray ray = GetRay(i, j);
                         pixelColor += RayColor(ray, MaxDepth, world);
                     }
                     string pixelOutput = (pixelColor * pixelSamplesScale).ToColor();
                     image[j * ImageWidth + i] = pixelOutput;
                 });
                 Interlocked.Increment(ref linesProcessed);
                 Console.WriteLine($"Scanlines remaining: {imageHeight - linesProcessed}\n");
             });

            string output = $"P3\n {ImageWidth} {imageHeight}\n255\n" + string.Join("\n", image);

            return output;
        }
    }
}
