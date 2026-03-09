using Raytracer;
using SkiaSharp;
using System;

public class RtwImage
{
    private byte[] data;
    private int imageWidth;
    private int imageHeight;
    private const int BytesPerPixel = 3;

    public int Width => data == null ? 0 : imageWidth;
    public int Height => data == null ? 0 : imageHeight;

    public RtwImage(string imageFilename)
    {
        string[] candidates = {
            imageFilename,
            "images/" + imageFilename,
            "../images/" + imageFilename,
            "../../images/" + imageFilename,
        };

        foreach (var path in candidates)
        {
            if (Load(path)) return;
        }

        Console.Error.WriteLine($"ERROR: Could not load image file '{imageFilename}'.");
    }

    private bool Load(string filename)
    {
        try
        {
            using var bitmap = SKBitmap.Decode(filename);
            if (bitmap == null) return false;

            imageWidth = bitmap.Width;
            imageHeight = bitmap.Height;
            data = new byte[imageWidth * imageHeight * BytesPerPixel];

            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    SKColor pixel = bitmap.GetPixel(x, y);
                    int idx = (y * imageWidth + x) * BytesPerPixel;
                    // Anti-gamma correction: convert from sRGB to linear color space by undoing the 2.2 power curve applied by most image formats. This is necessary to get correct lighting when using the image as a texture.
                    data[idx] = (byte)(Math.Pow(pixel.Red / 255.0, 2.2) * 255);
                    data[idx + 1] = (byte)(Math.Pow(pixel.Green / 255.0, 2.2) * 255);
                    data[idx + 2] = (byte)(Math.Pow(pixel.Blue / 255.0, 2.2) * 255);
                }
            }
            return true;
        }
        catch { return false; }
    }

    public (byte r, byte g, byte b) PixelData(int x, int y)
    {
        if (data == null) return (255, 0, 255); // magenta fallback

        x = (int)new Interval(0, imageWidth - 1).Clamp(x);
        y = (int)new Interval(0, imageHeight - 1).Clamp(y);

        int idx = (y * imageWidth + x) * BytesPerPixel;
        return (data[idx], data[idx + 1], data[idx + 2]);
    }
}