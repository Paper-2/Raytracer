using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ICS488_HW1
{
    internal class Program
    {
        int image_width;
        int image_height;
        Vector3[] image;

        bool antiAlias { get; set; } = false;
        bool blend { get; set; } = false;

        public Program(int width, int height)
        {
            image_width = width;
            image_height = height;
            image = new Vector3[image_width * image_height];
        }

        void Practice()
        {
            string output = $"P3\n {image_width} {image_height}\n255\n";

            for (int j = 0; j < image_height; j++)
            {
                for (int i = 0; i < image_width; i++)
                {
                    var r = (double)i / (image_width - 1);
                    var g = (double)j / (image_height - 1);
                    var b = 0.0;

                    int ir = (int)(255.999 * r);
                    int ig = (int)(255.999 * g);
                    int ib = (int)(255.999 * b);

                    output += ir + " " + ig + " " + ib + "\n";
                }
            }
            File.WriteAllText("output.ppm", output);
        }

        void WriteFile(string filename = "output.ppm")
        {
            string output = $"P3\n {image_width} {image_height}\n255\n";
            output = image.Aggregate(output, (current, color) =>
            {
                int ir = Math.Max(Math.Min((int)(255.999 * color.X), 255), 0);
                int ig = Math.Max(Math.Min((int)(255.999 * color.Y), 255), 0);
                int ib = Math.Max(Math.Min((int)(255.999 * color.Z), 255), 0);
                return current + ir + " " + ig + " " + ib + "\n";
            });
            File.WriteAllText(filename, output);
        }

        void DrawPixel(int x, int y, Vector3 color)
        {
            if (blend)
            {
                BlendPixel(x, y, color);
            }
            else
            {
                SetPixel(x, y, color);
            }
        }

        void SetPixel(int x, int y, Vector3 color)
        {
            if (x < 0 || x >= image_width || y < 0 || y >= image_height)
                return;
            image[y * image_width + x] = color;
        }

        void BlendPixel(int x, int y, Vector3 color)
        {
            if (x < 0 || x >= image_width || y < 0 || y >= image_height)
                return;
            image[y * image_width + x] += color;
        }

        void DrawLine(Vector3 start, Vector3 end, Vector3 color)
        {
            if (antiAlias)
            {
                DrawLineAA(start, end, color);
            }
            else if (Math.Abs(end.X - start.X) > Math.Abs(end.Y - start.Y))
            {
                DrawLineH(start, end, color);
            }
            else
            {
                DrawLineV(start, end, color);
            }
        }

        void DrawLineH(Vector3 start, Vector3 end, Vector3 color)
        {
            int x0 = (int)start.X;
            int y0 = (int)start.Y;
            int x1 = (int)end.X;
            int y1 = (int)end.Y;

            if (x0 > x1)
            {
                // Flip the line if necessary to ensure left-to-right drawing
                int temp = x0; x0 = x1; x1 = temp;
                temp = y0; y0 = y1; y1 = temp;
            }

            int dx = x1 - x0;
            int dy = y1 - y0;

            int dir = (dy < 0) ? -1 : 1;
            dy *= dir;

            if (dx != 0)
            {
                int y = y0;
                int p = 2 * dy - dx;
                for (int i = 0; i <= dx; i++)
                {
                    SetPixel(x0 + i, y, color);
                    if (p >= 0)
                    {
                        y += dir;
                        p -= 2 * dx;
                    }
                    p += 2 * dy;
                }

            }
        }

        void DrawLineV(Vector3 start, Vector3 end, Vector3 color)
        {
            int x0 = (int)start.X;
            int y0 = (int)start.Y;
            int x1 = (int)end.X;
            int y1 = (int)end.Y;

            if (y0 > y1)
            {
                // Flip the line if necessary to ensure left-to-right drawing
                int temp = x0; x0 = x1; x1 = temp;
                temp = y0; y0 = y1; y1 = temp;
            }

            int dx = x1 - x0;
            int dy = y1 - y0;

            int dir = (dx < 0) ? -1 : 1;
            dx *= dir;

            if (dy != 0)
            {
                int x = x0;
                int p = 2 * dx - dy;
                for (int i = 0; i <= dy; i++)
                {
                    SetPixel(x, y0 + i, color);
                    if (p >= 0)
                    {
                        x += dir;
                        p -= 2 * dy;
                    }
                    p += 2 * dx;
                }

            }
        }

        void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 color)
        {
            DrawLine(v0, v1, color);
            DrawLine(v1, v2, color);
            DrawLine(v2, v0, color);
        }

        // Yoinked from https://en.wikipedia.org/wiki/Xiaolin_Wu%27s_line_algorithm
        static float fpart(float x) => x - (float)Math.Floor(x);
        static float rfpart(float x) => 1f - fpart(x);
        void DrawLineAA(Vector3 start, Vector3 end, Vector3 color)
        {
            bool steep = Math.Abs(end.Y - start.Y) > Math.Abs(end.X - start.X);
            float x0 = start.X, y0 = start.Y, x1 = end.X, y1 = end.Y;

            if (steep)
            {
                // swap x and y
                float t;
                t = x0; x0 = y0; y0 = t;
                t = x1; x1 = y1; y1 = t;
            }

            if (x0 > x1)
            {
                // swap endpoints
                float t;
                t = x0; x0 = x1; x1 = t;
                t = y0; y0 = y1; y1 = t;
            }

            float dx = x1 - x0;
            float dy = y1 - y0;
            float gradient = dx == 0f ? 1f : dy / dx;

            // handle first endpoint
            float xend = (float)Math.Floor(x0);
            float yend = y0 + gradient * (xend - x0);
            float xgap = 1 - x0 + xend;
            int xpxl1 = (int)xend;
            int ypxl1 = (int)Math.Floor(yend);

            if (steep)
            {
                DrawPixel(ypxl1, xpxl1, Vector3.Multiply(color, rfpart(yend) * xgap));
                DrawPixel(ypxl1 + 1, xpxl1, Vector3.Multiply(color, fpart(yend) * xgap));
            }
            else
            {
                DrawPixel(xpxl1, ypxl1, Vector3.Multiply(color, rfpart(yend) * xgap));
                DrawPixel(xpxl1, ypxl1 + 1, Vector3.Multiply(color, fpart(yend) * xgap));
            }

            float intery = yend + gradient;

            // handle second endpoint
            xend = (float)Math.Ceiling(x1);
            yend = y1 + gradient * (xend - x1);
            xgap = 1 - xend + x1;
            int xpxl2 = (int)xend;
            int ypxl2 = (int)Math.Floor(yend);

            if (steep)
            {
                DrawPixel(ypxl2, xpxl2, Vector3.Multiply(color, rfpart(yend) * xgap));
                DrawPixel(ypxl2 + 1, xpxl2, Vector3.Multiply(color, fpart(yend) * xgap));
            }
            else
            {
                DrawPixel(xpxl2, ypxl2, Vector3.Multiply(color, rfpart(yend) * xgap));
                DrawPixel(xpxl2, ypxl2 + 1, Vector3.Multiply(color, fpart(yend) * xgap));
            }

            // main loop
            if (steep)
            {
                for (int x = xpxl1 + 1; x <= xpxl2 - 1; x++)
                {
                    DrawPixel((int)Math.Floor(intery), x, Vector3.Multiply(color, rfpart(intery)));
                    DrawPixel((int)Math.Floor(intery) + 1, x, Vector3.Multiply(color, fpart(intery)));
                    intery += gradient;
                    //Console.WriteLine($"Steep Line AA Pixel at x={x}, y={intery}, rfpart={rfpart(intery)}");
                }
            }
            else
            {
                for (int x = xpxl1 + 1; x <= xpxl2 - 1; x++)
                {
                    DrawPixel(x, (int)Math.Floor(intery), Vector3.Multiply(color, rfpart(intery)));
                    DrawPixel(x, (int)Math.Floor(intery) + 1, Vector3.Multiply(color, fpart(intery)));
                    intery += gradient;
                }
            }
        }

        void DrawTriangleFilled(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 color)
        {
            Vector3 temp;
            // Sort vertices by Y coordinate ascending (v0, v1, v2)
            if (v0.Y > v1.Y) { temp = v0; v0 = v1; v1 = temp; }
            if (v0.Y > v2.Y) { temp = v0; v0 = v2; v2 = temp; }
            if (v1.Y > v2.Y) { temp = v1; v1 = v2; v2 = temp; }

            for (int y = (int)v0.Y; y <= (int)v2.Y; y++)
            {
                if (y < v1.Y)
                {
                    // Lower part of the triangle
                    float alpha = (y - v0.Y) / (v1.Y - v0.Y);
                    float beta = (y - v0.Y) / (v2.Y - v0.Y);
                    int ax = (int)(v0.X + (v1.X - v0.X) * alpha);
                    int bx = (int)(v0.X + (v2.X - v0.X) * beta);
                    if (ax > bx) { int t = ax; ax = bx; bx = t; }
                    for (int x = ax; x <= bx; x++)
                    {
                        DrawPixel(x, y, color);
                    }
                }
                else
                {
                    // Upper part of the triangle
                    float alpha = (y - v1.Y) / (v2.Y - v1.Y);
                    float beta = (y - v0.Y) / (v2.Y - v0.Y);
                    int ax = (int)(v1.X + (v2.X - v1.X) * alpha);
                    int bx = (int)(v0.X + (v2.X - v0.X) * beta);
                    if (ax > bx) { int t = ax; ax = bx; bx = t; }
                    for (int x = ax; x <= bx; x++)
                    {
                        DrawPixel(x, y, color);
                    }
                }
            }
        }

        void DrawCircleFilled(Vector3 center, float radius, Vector3 color)
        {
            int cx = (int)Math.Round(center.X);
            int cy = (int)Math.Round(center.Y);
            int r = (int)Math.Ceiling(radius);

            for (int y = cy - r; y <= cy + r; y++)
            {
                int dy = y - cy;
                float dxMaxF = (float)Math.Sqrt(Math.Max(0.0, radius * radius - dy * dy));
                int x0 = (int)Math.Ceiling(cx - dxMaxF);
                int x1 = (int)Math.Floor(cx + dxMaxF);
                for (int x = x0; x <= x1; x++)
                {
                    DrawPixel(x, y, color);
                }
            }
        }

        static Vector3 black = new Vector3(0f, 0f, 0f);
        static Vector3 white = new Vector3(1f, 1f, 1f);
        static Vector3 gray = new Vector3(0.5f, 0.5f, 0.5f);
        static Vector3 red = new Vector3(1f, 0f, 0f);
        static Vector3 green = new Vector3(0f, 1f, 0f);
        static Vector3 blue = new Vector3(0f, 0f, 1f);
        static Vector3 yellow = new Vector3(1f, 1f, 0f);
        static Vector3 cyan = new Vector3(0f, 1f, 1f);
        static Vector3 magenta = new Vector3(1f, 0f, 1f);
        static Program p;
        static void Main(string[] args)
        {
            p = new Program(256, 256);
            p.DrawTriangle(new Vector3(30, 30, 0),
                new Vector3(200, 50, 0),
                new Vector3(100, 200, 0),
                white);
            p.WriteFile("triangle_outline.ppm");

            p = new Program(256, 256);
            p.antiAlias = true;
            p.DrawTriangle(new Vector3(30, 30, 0),
                new Vector3(200, 50, 0),
                new Vector3(100, 200, 0),
                white);
            p.WriteFile("triangle_outlineAA.ppm");

            p = new Program(256, 256);
            p.DrawTriangleFilled(new Vector3(30, 30, 0),
                new Vector3(200, 50, 0),
                new Vector3(100, 200, 0),
                gray);
            p.WriteFile("triangle_filled.ppm");
            p.DrawTriangleFilled(new Vector3(50, 100, 0),
                new Vector3(220, 80, 0),
                new Vector3(150, 220, 0),
                red);
            p.DrawTriangleFilled(new Vector3(80, 50, 0),
                new Vector3(180, 30, 0),
                new Vector3(130, 180, 0),
                green);
            p.WriteFile("triangle_filled_multiple.ppm");

            p = new Program(256, 256);
            p.blend = true;
            p.DrawTriangleFilled(new Vector3(30, 30, 0),
                new Vector3(200, 50, 0),
                new Vector3(100, 200, 0),
                gray);
            p.DrawTriangleFilled(new Vector3(50, 100, 0),
                new Vector3(220, 80, 0),
                new Vector3(150, 220, 0),
                red);
            p.DrawTriangleFilled(new Vector3(80, 50, 0),
                new Vector3(180, 30, 0),
                new Vector3(130, 180, 0),
                green);
            p.WriteFile("triangle_filled_multiple_blend.ppm");

            p = new Program(256, 256);
            p.DrawCircleFilled(new Vector3(64, 150, 0), 80f, cyan);
            p.WriteFile("circle_filled.ppm");
        }
    }
}
