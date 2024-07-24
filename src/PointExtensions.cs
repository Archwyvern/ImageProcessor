using SixLabors.ImageSharp;
using System;
using System.Numerics;

namespace Archwyvern.Space2D.ImageProcessor;

public static class PointExtensions
{
    public static float DistanceTo(this Point p1, Point p2)
    {
        return (float)Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
    }

    public static float DistanceToSquared(this Point p1, Point p2)
    {
        return (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);
    }

    public static bool Contains(this Rectangle rect, Vector2 vector)
    {
        return rect.Contains((int)vector.X, (int)vector.Y);
    }
}