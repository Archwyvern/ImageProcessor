using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Archwyvern.Space2D.ImageProcessor;

internal class QuadTree : IPointIndex
{
    private class QuadTreeNode
    {
        private const int Capacity = 20;
        private bool Divided = false;

        public Rectangle Bounds { get; private set; }
        public List<Vector2> Points { get; private set; }
        public QuadTreeNode[] Children { get; private set; }

        public QuadTreeNode(Rectangle bounds)
        {
            Bounds = bounds;
            Points = new List<Vector2>();
            Children = new QuadTreeNode[4];
        }

        public void Subdivide()
        {
            int x = Bounds.X;
            int y = Bounds.Y;
            int w = Bounds.Width / 2;
            int h = Bounds.Height / 2;

            Children[0] = new QuadTreeNode(new Rectangle(x, y, w, h));
            Children[1] = new QuadTreeNode(new Rectangle(x + w, y, w, h));
            Children[2] = new QuadTreeNode(new Rectangle(x, y + h, w, h));
            Children[3] = new QuadTreeNode(new Rectangle(x + w, y + h, w, h));

            Divided = true;
        }

        public bool Insert(Vector2 point)
        {
            if (!Bounds.Contains(point))
                return false;

            if (Points.Count < Capacity)
            {
                Points.Add(point);
                return true;
            }

            if (!Divided)
            {
                Subdivide();
            }

            foreach (var child in Children)
            {
                if (child.Insert(point))
                    return true;
            }

            return false;
        }

        private object _lock = new object();

        public bool FindNearestPoint(Vector2 queryPoint, float maxDistanceSquared, ref Vector2 nearestPoint, ref float nearestDistanceSquared)
        {
            bool found = false;

            QuadTreeNode[] children;

            lock (_lock)
            {
                children = (QuadTreeNode[])Children.Clone();

                if (!Bounds.IntersectsWith(new Rectangle(
                    (int)queryPoint.X - (int)MathF.Sqrt(maxDistanceSquared), 
                    (int)queryPoint.Y - (int)MathF.Sqrt(maxDistanceSquared), 
                    (int)(2 * MathF.Sqrt(maxDistanceSquared)), 
                    (int)(2 * MathF.Sqrt(maxDistanceSquared))
                ))) {
                    return false;
                }

                foreach (var point in Points)
                {
                    var currentDistanceSquared = Vector2.DistanceSquared(point, queryPoint);
                    if (currentDistanceSquared < nearestDistanceSquared && currentDistanceSquared <= maxDistanceSquared)
                    {
                        nearestDistanceSquared = currentDistanceSquared;
                        nearestPoint = point;
                        found = true;
                    }
                }
            }

            if (Divided)
            {
                foreach (var child in children)
                {
                    found |= child.FindNearestPoint(queryPoint, maxDistanceSquared, ref nearestPoint, ref nearestDistanceSquared);
                }
            }

            return found;
        }
    }

    private QuadTreeNode _root;

    public QuadTree(IEnumerable<Vector2> points, Rectangle bounds)
    {
        int size = 1024;
        int majorAxis = Math.Max(bounds.Width, bounds.Height);

        while (size < majorAxis) {
            size *= 2;
        }

        _root = new QuadTreeNode(new Rectangle(0, 0, size, size));

        foreach (var point in points)
        {
            _root.Insert(point);
        }
    }

    public void Insert(Point point)
    {
        _root.Insert(point);
    }

    public bool FindNearestPoint(Vector2 queryPoint, float maxDistance, out Vector2 nearestPoint)
    {
        nearestPoint = Vector2.Zero;
        float nearestDistanceSquared = maxDistance * maxDistance;

        return _root.FindNearestPoint(queryPoint, nearestDistanceSquared, ref nearestPoint, ref nearestDistanceSquared);
    }

    public bool FindNearestPoints(Point queryPoint, float maxPoints, float maxDistance, out Point[] nearestPoint)
    {
        throw new NotImplementedException();
    }
}