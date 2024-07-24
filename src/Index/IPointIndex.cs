using SixLabors.ImageSharp;
using System;
using System.Numerics;

namespace Archwyvern.Space2D.ImageProcessor;


internal interface IPointIndex
{
    public bool FindNearestPoint(Vector2 queryPoint, float maxDistance, out Vector2 nearestPoint);
}