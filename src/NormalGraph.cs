using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Archwyvern.Space2D.ImageProcessor;

internal partial class NormalGraph
{
    public static readonly RgbaVector Neutral = new(0.5f, 0.5f, 1, 1);

    // TODO: allow X and Y flipping
    public static RgbaVector GetColor(Vector3 normal)
    {
        if (normal.Length() == 0) {
            return Neutral;
        }

        float r = Math.Clamp(0.5f + normal.X / 2, 0, 1);
        float g = Math.Clamp(0.5f - normal.Y / 2, 0, 1);
        float b = 1 - normal.Z;

        return new RgbaVector(r, g, b, 1f);
    }
}
