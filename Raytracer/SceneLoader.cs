using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    /// <summary>
    /// Loads triangle scenes exported from VulkanRaymarcher
    /// </summary>
    public static class SceneLoader
    {
        /// <summary>
        /// Material types matching VulkanRaymarcher GpuMaterial
        /// </summary>
        public const uint LAMBERTIAN = 0;
        public const uint METAL = 1;
        public const uint DIELECTRIC = 2;
        public const uint DIFFUSE_LIGHT = 3;

        /// <summary>
        /// Loads a scene from a text file exported by VulkanRaymarcher
        /// </summary>
        public static void LoadScene(string filePath, HittableList world, out List<Material> materials)
        {
            materials = new List<Material>();
            var culture = CultureInfo.InvariantCulture;
            var lines = File.ReadAllLines(filePath);
            
            List<Material> materialList = new List<Material>();
            int triangleCount = 0;
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;
                
                var parts = trimmed.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;
                
                switch (parts[0])
                {
                    case "materials":
                        // Just a count hint, materials follow
                        break;
                        
                    case "mat":
                        // mat index albedoR albedoG albedoB type fuzz refraction emissionR emissionG emissionB
                        if (parts.Length >= 11)
                        {
                            int idx = int.Parse(parts[1]);
                            double albedoR = double.Parse(parts[2], culture);
                            double albedoG = double.Parse(parts[3], culture);
                            double albedoB = double.Parse(parts[4], culture);
                            uint type = uint.Parse(parts[5]);
                            double fuzz = double.Parse(parts[6], culture);
                            double refraction = double.Parse(parts[7], culture);
                            double emissionR = double.Parse(parts[8], culture);
                            double emissionG = double.Parse(parts[9], culture);
                            double emissionB = double.Parse(parts[10], culture);
                            
                            Vec3 albedo = new Vec3(albedoR, albedoG, albedoB);
                            Vec3 emission = new Vec3(emissionR, emissionG, emissionB);
                            
                            Material mat;
                            switch (type)
                            {
                                case LAMBERTIAN:
                                    mat = new Lambertian(albedo);
                                    break;
                                case METAL:
                                    mat = new Metal(albedo, fuzz);
                                    break;
                                case DIELECTRIC:
                                    mat = new Dielectric(refraction);
                                    break;
                                case DIFFUSE_LIGHT:
                                    mat = new DiffuseLight(emission);
                                    break;
                                default:
                                    mat = new Lambertian(albedo);
                                    break;
                            }
                            
                            // Ensure list is large enough
                            while (materialList.Count <= idx)
                                materialList.Add(null);
                            materialList[idx] = mat;
                        }
                        break;
                        
                    case "triangles":
                        // Just a count hint
                        break;
                        
                    case "tri":
                        // tri v0x v0y v0z v1x v1y v1z v2x v2y v2z materialIndex
                        if (parts.Length >= 11)
                        {
                            double v0x = double.Parse(parts[1], culture);
                            double v0y = double.Parse(parts[2], culture);
                            double v0z = double.Parse(parts[3], culture);
                            double v1x = double.Parse(parts[4], culture);
                            double v1y = double.Parse(parts[5], culture);
                            double v1z = double.Parse(parts[6], culture);
                            double v2x = double.Parse(parts[7], culture);
                            double v2y = double.Parse(parts[8], culture);
                            double v2z = double.Parse(parts[9], culture);
                            int matIdx = int.Parse(parts[10]);
                            
                            Vec3 v0 = new Vec3(v0x, v0y, v0z);
                            Vec3 v1 = new Vec3(v1x, v1y, v1z);
                            Vec3 v2 = new Vec3(v2x, v2y, v2z);
                            
                            Material mat = matIdx < materialList.Count && materialList[matIdx] != null 
                                ? materialList[matIdx] 
                                : new Lambertian(new Vec3(0.5, 0.5, 0.5));
                            
                            world.Add(new Triangle(v0, v1, v2, mat));
                            triangleCount++;
                        }
                        break;
                }
            }
            
            materials = materialList;
            Console.WriteLine($"Loaded {triangleCount} triangles and {materialList.Count} materials from {filePath}");
        }
    }
}
