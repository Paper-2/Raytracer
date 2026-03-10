using SharpNoise.Modules;
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

            Hittable box1 = Quad.Box(new Vec3(0, 0, 0), new Vec3(165, 330, 165), white);
            box1 = new RotateY(box1, 15);
            box1 = new Translate(box1, new Vec3(265, 0, 295));
            world.Add(box1);

            Hittable box2 = Quad.Box(new Vec3(0, 0, 0), new Vec3(165, 165, 165), white);
            box2 = new RotateY(box2, -18);
            box2 = new Translate(box2, new Vec3(130, 0, 65));
            world.Add(box2);


            cam = new Camera(
                aspectRatio: 1.0,
                imageWidth: 300,
                samplesPerPixel: 50,
                maxDepth: 10,
                background: new Vec3(0, 0, 0),
                vFov: 40,
                lookFrom: new Vec3(278, 278, -800),
                lookAt: new Vec3(278, 278, 0),
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0);
        }

        static void CornellSmoke(HittableList world, out Camera cam)
        {
            Material red = new Lambertian(new Vec3(0.65, 0.05, 0.05));
            Material white = new Lambertian(new Vec3(0.73, 0.73, 0.73));
            Material green = new Lambertian(new Vec3(0.12, 0.45, 0.15));
            Material light = new DiffuseLight(new Vec3(7, 7, 7));
            world.Add(new Quad(new Vec3(555, 0, 0), new Vec3(0, 555, 0), new Vec3(0, 0, 555), green));
            world.Add(new Quad(new Vec3(0, 0, 0), new Vec3(0, 555, 0), new Vec3(0, 0, 555), red));
            world.Add(new Quad(new Vec3(113, 554, 127), new Vec3(330, 0, 0), new Vec3(0, 0, 305), light));
            world.Add(new Quad(new Vec3(0, 555, 0), new Vec3(555, 0, 0), new Vec3(0, 0, 555), white));
            world.Add(new Quad(new Vec3(0, 0, 0), new Vec3(555, 0, 0), new Vec3(0, 0, 555), white));
            world.Add(new Quad(new Vec3(0, 0, 555), new Vec3(555, 0, 0), new Vec3(0, 555, 0), white));

            Hittable box1 = Quad.Box(new Vec3(0, 0, 0), new Vec3(165, 330, 165), white);
            box1 = new RotateY(box1, 15);
            box1 = new Translate(box1, new Vec3(265, 0, 295));

            Hittable box2 = Quad.Box(new Vec3(0, 0, 0), new Vec3(165, 165, 165), white);
            box2 = new RotateY(box2, -18);
            box2 = new Translate(box2, new Vec3(130, 0, 65));

            world.Add(new ConstantMedium(box1, 0.01, new Vec3(0, 0, 0)));
            world.Add(new ConstantMedium(box2, 0.01, new Vec3(1, 1, 1)));


            cam = new Camera(
                aspectRatio: 1.0,
                imageWidth: 300,
                samplesPerPixel: 50,
                maxDepth: 10,
                background: new Vec3(0, 0, 0),
                vFov: 40,
                lookFrom: new Vec3(278, 278, -800),
                lookAt: new Vec3(278, 278, 0),
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0);
        }

        static void FinalScene(HittableList world, out Camera cam)
        {
            HittableList boxes1 = new HittableList();
            Material ground = new Lambertian(new Vec3(0.48, 0.83, 0.53));

            Random rand = new Random();
            int boxesPerSide = 20;
            for (int i = 0; i < boxesPerSide; i++)
            {
                for (int j = 0; j < boxesPerSide; j++)
                {
                    double w = 100;
                    double x0 = -1000 + i * w;
                    double z0 = -1000 + j * w;
                    double y0 = 0;
                    double x1 = x0 + w;
                    double y1 = rand.NextDouble() * 101 + 1;
                    double z1 = z0 + w;
                    boxes1.Add(Quad.Box(new Vec3(x0, y0, z0), new Vec3(x1, y1, z1), ground));
                }
            }

            world.Add(new BVHNode(boxes1));

            Material light = new DiffuseLight(new Vec3(7, 7, 7));
            world.Add(new Quad(new Vec3(123, 554, 147), new Vec3(300, 0, 0), new Vec3(0, 0, 265), light));

            Vec3 center1 = new Vec3(400, 400, 200);
            Vec3 center2 = center1 + new Vec3(30, 0, 0);
            Material movingSphereMaterial = new Lambertian(new Vec3(0.7, 0.3, 0.1));
            world.Add(new Sphere(center1, center2, 50, movingSphereMaterial));

            world.Add(new Sphere(new Vec3(260, 150, 45), 50, new Dielectric(1.5)));
            world.Add(new Sphere(new Vec3(0, 150, 145), 50, new Metal(new Vec3(0.8, 0.8, 0.9), 1.0)));

            Hittable boundary = new Sphere(new Vec3(360, 150, 145), 70, new Dielectric(1.5));
            world.Add(boundary);
            world.Add(new ConstantMedium(boundary, 0.2, new Vec3(0.2, 0.4, 0.9)));
            boundary = new Sphere(new Vec3(0, 0, 0), 5000, new Dielectric(1.5));
            world.Add(new ConstantMedium(boundary, 0.0001, new Vec3(1, 1, 1)));

            Material earthMaterial = new Lambertian(new ImageTexture("earthmap.jpg"));
            world.Add(new Sphere(new Vec3(400, 200, 400), 100, earthMaterial));
            Material perlinMaterial = new Lambertian(new NoiseTexture(0.2));
            world.Add(new Sphere(new Vec3(220, 280, 300), 80, perlinMaterial));

            HittableList boxes2 = new HittableList();
            Material white = new Lambertian(new Vec3(0.73, 0.73, 0.73));
            int ns = 1000;
            for (int j = 0; j < ns; j++)
            {
                boxes2.Add(new Sphere(Vec3.Random(0, 165), 10, white));
            }

            world.Add(new Translate(new RotateY(new BVHNode(boxes2), 15), new Vec3(-100, 270, 395)));

            cam = new Camera(
                aspectRatio: 1.0,
                imageWidth: 1080,
                samplesPerPixel: 100,
                maxDepth: 50,
                background: new Vec3(0, 0, 0),
                vFov: 40,
                lookFrom: new Vec3(478, 278, -600),
                lookAt: new Vec3(278, 278, 0),
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0);
        }

        static void EerieScene(HittableList world, out Camera cam, out string filename)
        {
            // Ground
            Material ground = new Lambertian(new Vec3(0.46, 0.36, 0.29));
            world.Add(new Sphere(new Vec3(0, -1000, 0), 1000, ground));

            // Towers
            Random rand = new Random(67);
            HittableList towers = new HittableList();
            for (int a = -20; a < 20; a += 2)
            {
                for (int b = 2; b < 40; b += 2)
                {
                    //if (a >= -2 && a <= 2) continue;
                    Material metal = new Metal(new Vec3(0.3, 0.3, 0.4), 0.8);
                    Hittable box = Quad.Box(new Vec3(0, 0, 0), new Vec3(0.3, 10, 0.3), metal);
                    box = new RotateY(box, rand.NextDouble() * 90);
                    Vec3 position = new Vec3(a + 0.9 * rand.NextDouble(), -2, b + 0.9 * rand.NextDouble());
                    box = new Translate(box, position);
                    towers.Add(box);
                }
            }
            world.Add(new BVHNode(towers));

            // Warm torch light
            Material warmLight = new DiffuseLight(new Vec3(8, 4, 1));
            world.Add(new Sphere(new Vec3(-0.2, 0, 3), 0.13, warmLight));

            // Fog
            Hittable boundary = Quad.Box(new Vec3(-20, -10, 0), new Vec3(20, 10, 40), ground);
            world.Add(new ConstantMedium(boundary, 0.01, new Vec3(1, 1, 1)));

            // Figure
            HittableList figure = new HittableList();
            Material figureMat = new Lambertian(new Vec3(1, 1, 1));
            Vec3 translate = new Vec3(-0.07, 0, 25);
            //Vec3 translate = new Vec3(-0.07, -0.5, 3);

            Hittable head = new Sphere(new Vec3(0, 0.9, 0), 0.2, figureMat);
            figure.Add(head);
            Hittable body = Quad.Box(new Vec3(-0.1, -2, 0), new Vec3(0.15, 1.1, 0.15), figureMat);
            figure.Add(body);

            //Material eyeMaterial = new DiffuseLight(new Vec3(1, 1, 0));
            //Hittable leftEye = new Sphere(new Vec3(0.07, 0.95, -0.17), 0.02, eyeMaterial);
            //figure.Add(leftEye);
            //Hittable rightEye = new Sphere(new Vec3(-0.07, 0.95, -0.17), 0.02, eyeMaterial);
            //figure.Add(rightEye);

            world.Add(new Translate(new BVHNode(figure), translate));

            cam = new Camera(
                aspectRatio: 16.0 / 9.0,
                imageWidth: 1920,
                samplesPerPixel: 100,
                maxDepth: 50,
                background: new Vec3(0.01, 0.01, 0.01),
                vFov: 20,
                lookFrom: new Vec3(0, 0.5, 0),
                lookAt: new Vec3(0.1, 0.45, 1),
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0.6,
                focusDist: 10);
            filename = "eerie_scene.ppm";
        }

        static void Face(HittableList world, out Camera cam, out string filename)
        {
            Texture faceTexture = new ImageTexture("face.png");
            Material faceMaterial = new Lambertian(faceTexture);

            double spacing = 5;
            for (int x = 0; x <= 32; x += 3) 
            {
                Hittable sphere = new Sphere(new Vec3(0, 0, 0), x * 0.03 + 1, faceMaterial);
                for (int y = -8; y <= 8; y++)
                {
                    for (int z = -8; z <= 8; z++)
                    { 
                        world.Add(new Translate(sphere, new Vec3(-(x * spacing + 10), y * spacing, z * spacing))); 
                    }
                }
            }

            Material glass = new Dielectric(1.5);
            Random rand = new Random(67);

            for (int i = 0; i < 5; i++)
            {
                double gx = -(rand.NextDouble() * 32 * spacing + 10);
                double gy = (rand.NextDouble() * 16 - 8) * spacing;
                double gz = (rand.NextDouble() * 16 - 8) * spacing;
                double gr = 1 + rand.NextDouble() * 2.0;
                world.Add(new Sphere(new Vec3(gx, gy, gz) * 0.25, gr, glass));
            }

            cam = new Camera(
                aspectRatio: 16.0 / 9.0,
                imageWidth: 1920,
                samplesPerPixel: 100,
                maxDepth: 50,
                background: new Vec3(1, 1, 1),
                vFov: 20,
                lookFrom: new Vec3(12, 0, 0),
                lookAt: new Vec3(0, 0, 0),
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0);

            filename = "face.ppm";
        }

        static void LoadExportedScene(string filePath, HittableList world, out Camera cam)
        {
            List<Material> materials;
            SceneLoader.LoadScene(filePath, world, out materials);
            
            // Camera matching VulkanRaymarcher's LoadGltfScene
            cam = new Camera(
                aspectRatio: 4.0 / 3.0,
                imageWidth: 800,
                samplesPerPixel: 10,
                maxDepth: 50,
                background: new Vec3(0.02, 0.02, 0.03),
                vFov: 50,
                lookFrom: new Vec3(0, 4, -8),
                lookAt: new Vec3(0, 0, 0),
                vUp: new Vec3(0, 1, 0),
                defocusAngle: 0);
        }

        static void Main(string[] args)
        {
            HittableList scene = new HittableList();
            string filename = null;

            LoadExportedScene(@"c:\Users\holac\source\repos\Raytracer\VulkanRaymarcher\scene_triangles.txt", scene, out Camera cam);
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
            if (filename == null)
                filename = "output.ppm";

            var sw = Stopwatch.StartNew();
            string output = cam.Render(world);
            sw.Stop();

            Console.WriteLine($"Render completed in {sw.Elapsed.TotalSeconds:F2} seconds ({sw.Elapsed}).");
            File.WriteAllText(filename, output);
        }
    }
}
