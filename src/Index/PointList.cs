using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SixLabors.ImageSharp;

namespace Archwyvern.Space2D.ImageProcessor;

// For testing only
internal class PointList : IPointIndex
{
    private List<Point> _points = [];

    public PointList(IEnumerable<Point> points)
    {
        foreach (var point in points) {
            _points.Add(point);
        }
    }

    public bool FindNearestPoint(Vector2 queryPoint, float maxDistance, out Vector2 nearestPoint)
    {
        var min = float.MaxValue;
        nearestPoint = Point.Empty;
        var found = false;

        maxDistance *= maxDistance;

        foreach (var point in _points) {
            var currentDistance = Vector2.DistanceSquared(point, queryPoint);

            if (currentDistance <= maxDistance && currentDistance < min) {
                min = currentDistance;
                nearestPoint = point;
                found = true;
            }
        }

        return found;
    }
}