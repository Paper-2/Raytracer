using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Raytracer
{
    internal class Program
    {
        static void Scene0(HittableList world, out Camera cam)
        {
            cam = null;
            Material materialGround = new Lambertian(new Vec3(0.8, 0.8, 0.0));
            Material materialCenter = new Lambertian(new Vec3(0.1, 0.2, 0.5));
            Material materialLeft = new Dielectric(1.5);
            Material materialBubble = new Dielectric(1 / 1.5);
            Material materialRight = new Metal(new Vec3(0.8, 0.6, 0.2), 1.0);

            world.Add(new Sphere(new Vec3(0, -100.5, -1), 100, materialGround));
            world.Add(new Sphere(new Vec3(0, 0, -1.2), 0.5, materialCenter));
            world.Add(new Sphere(new Vec3(-1, 0, -1), 0.5, materialLeft));
            world.Add(new Sphere(new Vec3(-1, 0, -1), 0.4, materialBubble));
            world.Add(new Sphere(new Vec3(1, 0, -1), 0.5, materialRight));
        }

        static void Spheres(HittableList world, out Camera cam)
        {
            cam = null;
            Texture checker = new CheckerTexture(0.32, new Vec3(0.2, 0.3, 0.1), new Vec3(0.9, 0.9, 0.9));
            world.Add(new Sphere(new Vec3(0, -1000, 0), 1000, new Lambertian(checker)));

            Random rand = new Random();
            for (int a = -11; a < 11; a++)
            {
                for (int b = -11; b < 11; b++)
                {
                    double chooseMat = rand.NextDouble();
                    Vec3 center = new Vec3(a + 0.9 * rand.NextDouble(), 0.2, b + 0.9 * rand.NextDouble());

                    if ((center - new Vec3(4, 0.2, 0)).Length() > 0.9)
                    {
                        Material sphereMaterial;
                        if (chooseMat < 0.8)
                        {
                            // diffuse
                            sphereMaterial = new Lambertian(Vec3.Random() * Vec3.Random());
                            Vec3 center2 = center + new Vec3(0, rand.NextDouble() * 0.5, 0);
                            world.Add(new Sphere(center, center2, 0.2, sphereMaterial));
                        }
                        else if (chooseMat < 0.95)
                        {
                            // metal
                            sphereMaterial = new Metal(Vec3.Random(0.5, 1), rand.NextDouble() * 0.5);
                            world.Add(new Sphere(center, 0.2, sphereMaterial));
                        }
                        else
                        {
                            // glass
                            sphereMaterial = new Dielectric(1.5);
                            world.Add(new Sphere(center, 0.2, sphereMaterial));
                        }
                    }
                }
            }

            Material material1 = new Dielectric(1.5);
            world.Add(new Sphere(new Vec3(0, 1, 0), 1.0, material1));
            Material material2 = new Lambertian(new Vec3(0.4, 0.2, 0.1));
            world.Add(new Sphere(new Vec3(-4, 1, 0), 1.0, material2));
            Material material3 = new Metal(new Vec3(0.7, 0.6, 0.5), 0.0);
            world.Add(new Sphere(new Vec3(4, 1, 0), 1.0, material3));
        }

        static void Checkered(HittableList world, out Camera cam)
        {
            cam = null;
            Texture checker = new CheckerTexture(0.32, new Vec3(0.2, 0.3, 0.1), new Vec3(0.9, 0.9, 0.9));
            
            world.Add(new Sphere(new Vec3(0, -10, 0), 10, new Lambertian(checker)));
            world.Add(new Sphere(new Vec3(0, 10, 0), 10, new Lambertian(checker)));
        }

        static void Earth(HittableList world, out Camera cam)
        {
            cam = null;
            Texture earthTexture = new ImageTexture("earthmap.jpg");
            Material earthMaterial = new Lambertian(earthTexture);
            world.Add(new Sphere(new Vec3(0, 0, 0), 2, earthMaterial));
        }

        static void PerlinSpheres(HittableList world, out Camera cam)
        {
            cam = null;
            Texture perlinTexture = new NoiseTexture(4);
            world.Add(new Sphere(new Vec3(0, -1000, 0), 1000, new Lambertian(perlinTexture)));
            world.Add(new Sphere(new Vec3(0, 2, 0), 2, new Lambertian(perlinTexture)));
        }

        static void Quads(HittableList world, out Camera cam)
        {
            Material leftRed = new Lambertian(new Vec3(1.0, 0.2, 0.2));
            Material backGreen = new Lambertian(new Vec3(0.2, 1.0, 0.2));
            Material rightBlue = new Lambertian(new Vec3(0.2, 0.2, 1.0));
            Material upperOrange = new Lambertian(new Vec3(1.0, 0.5, 0.0));
            Material lowerTeal = new Lambertian(new Vec3(0.2, 0.8, 0.8));

            world.Add(new Quad(new Vec3(-3, -2, 5), new Vec3(0, 0, -4), new Vec3(0, 4, 0), leftRed));
            world.Add(new Quad(new Vec3(-2, -2, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0), backGreen));
            world.Add(new Quad(new Vec3(3, -2, 1), new Vec3(0, 0, 4), new Vec3(0, 4, 0), rightBlue));
            world.Add(new Quad(new Vec3(-2, 3, 1), new Vec3(4, 0, 0), new Vec3(0, 0, 4), upperOrange));
            world.Add(new Quad(new Vec3(-2, -3, 5), new Vec3(4, 0, 0), new Vec3(0, 0, -4), lowerTeal));

            cam = new Camera(
                aspectRatio: 1.0,
                imageWidth: 480,
                samplesPerPixel: 10,
                maxDepth: 10,
                vFov: 80,
                lookFrom: new Vec3(0,0,9),
                lookAt: new Vec3(0, 0, 0),
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0,
                focusDist: 10);
        }

        static void SimpleLight(HittableList world, out Camera cam)
        {
            Texture perlinTexture = new NoiseTexture(4);
            world.Add(new Sphere(new Vec3(0, -1000, 0), 1000, new Lambertian(perlinTexture)));
            world.Add(new Sphere(new Vec3(0, 2, 0), 2, new Lambertian(perlinTexture)));
            Texture lightTexture = new SolidColor(new Vec3(4, 4, 4));
            world.Add(new Quad(new Vec3(3, 1, -2), new Vec3(2, 0, 0), new Vec3(0, 2, 0), new DiffuseLight(lightTexture)));
            world.Add(new Sphere(new Vec3(0, 7, 0), 2, new DiffuseLight(lightTexture)));

            cam = new Camera(
                aspectRatio: 16.0 / 9.0,
                imageWidth: 400,
                samplesPerPixel: 100,
                maxDepth: 50,
                background: new Vec3(0, 0, 0),
                vFov: 20,
                lookFrom: new Vec3(26, 3, 6),
                lookAt: new Vec3(0, 2, 0),
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0,
                focusDist: 10);
        }

        static void CornellBox(HittableList world, out Camera cam)
        {
            Material red = new Lambertian(new Vec3(0.65, 0.05, 0.05));
            Material white = new Lambertian(new Vec3(0.73, 0.73, 0.73));
            Material green = new Lambertian(new Vec3(0.12, 0.45, 0.15));
            Material light = new DiffuseLight(new Vec3(15, 15, 15));
            world.Add(new Quad(new Vec3(555, 0, 0), new Vec3(0, 555, 0), new Vec3(0, 0, 555), green));
            world.Add(new Quad(new Vec3(0, 0, 0), new Vec3(0, 555, 0), new Vec3(0, 0, 555), red));
            world.Add(new Quad(new Vec3(343, 554, 332), new Vec3(-130, 0, 0), new Vec3(0, 0, -105), light));
            world.Add(new Quad(new Vec3(0, 0, 0), new Vec3(555, 0, 0), new Vec3(0, 0, 555), white));
            world.Add(new Quad(new Vec3(555, 555, 555), new Vec3(-555, 0, 0), new Vec3(0, 0, -555), white));
            world.Add(new Quad(new Vec3(0, 0, 555), new Vec3(555, 0, 0), new Vec3(0, 555, 0), white));

            Hittable box1 = Quad.Box(new Vec3(0, 0, 0), new Vec3(165, 330, 165), red);
            box1 = new RotateY(box1, 15);
            box1 = new Translate(box1, new Vec3(265, 0, 295));
            world.Add(box1);

            Hittable box2 = Quad.Box(new Vec3(0, 0, 0), new Vec3(165, 165, 165), green);
            box2 = new RotateY(box2, -18);
            box2 = new Translate(box2, new Vec3(130, 0, 65));
            world.Add(box2);


            cam = new Camera(
                aspectRatio: 1.0,
                imageWidth: 600,
                samplesPerPixel: 200,
                maxDepth: 50,
                background: new Vec3(0, 0, 0),
                vFov: 40,
                lookFrom: new Vec3(278, 278, -800),
                lookAt: new Vec3(278, 278, 0),
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0);
        }

        static void Main(string[] args)
        {
            HittableList scene = new HittableList();

            CornellBox(scene, out Camera cam);
            HittableList world = new HittableList();
            world.Add(new BVHNode(scene));

            if (cam == null)
                cam = new Camera(
                aspectRatio: 16.0 / 9.0, 
                imageWidth: 480, 
                samplesPerPixel: 10, 
                maxDepth: 10, 
                background: new Vec3(0.70, 0.80, 1.00),
                vFov: 20, 
                lookFrom: new Vec3(13, 2, 3), 
                lookAt: new Vec3(0, 0, 0), 
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0.6, 
                focusDist: 10);

            var sw = Stopwatch.StartNew();
            string output = cam.Render(world);
            sw.Stop();

            Console.WriteLine($"Render completed in {sw.Elapsed.TotalSeconds:F2} seconds ({sw.Elapsed}).");
            File.WriteAllText("output.ppm", output);
        }
    }
}
