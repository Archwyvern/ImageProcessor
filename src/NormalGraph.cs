using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Archwyvern.Space2D.ImageProcessor;

internal partial class NormalGraph
{
    public static readonly RgbaVector Neutral = new(0.5f, 0.5f, 1, 1);

    public static RgbaVector GetColor(Vector3 normal)
    {
        if (normal.Length() == 0) {
            return Neutral;
        }

        // Clamp the values to ensure they are within valid color range
        float r = Math.Clamp(0.5f + normal.X / 2, 0, 1);
        float g = Math.Clamp(0.5f - normal.Y / 2, 0, 1);
        float b = 1 - normal.Z; // Assuming blue component is always 1 for simplicity

        return new RgbaVector(r, g, b, 1f);
    }

    public static RgbaVector GetColor(Vector2 normal, float height = 0)
    {
        if (normal.Length() == 0) {
            return Neutral;
        }

        // Clamp the values to ensure they are within valid color range
        float r = Math.Clamp(0.5f + normal.X / 2, 0, 1);
        float g = Math.Clamp(0.5f - normal.Y / 2, 0, 1);
        float b = 1 - height; // Assuming blue component is always 1 for simplicity

        return new RgbaVector(r, g, b, 1f);
    }
}
