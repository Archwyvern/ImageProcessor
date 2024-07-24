using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Archwyvern.Space2D.ImageProcessor;

internal class ImageProcess
{
    public static readonly Point[] Directions = [
        new(1, 0),
        new(0, 1),
        new(-1, 0),
        new(0, -1),

        new(1, 1),
        new(-1, -1),
        new(-1, 1),
        new(1, -1)
    ];

    public Image<RgbaVector> Diffuse { get; }
    public Image<RgbaVector> Grayscale { get; }
    public IPointIndex Edges { get; }
    public int Width { get; }
    public int Height { get; }
    public float Opaque { get; }
    public float MinorAxis { get; }

    public Image<RgbaVector> Bevel { get; set; }

    public ImageProcess(string path)
    {
        Diffuse = Image.Load<RgbaVector>(path);
        Width = Diffuse.Width;
        Height = Diffuse.Height;
        Grayscale = Diffuse.Clone();

        var edges = new HashSet<Vector2>();

        var w = Diffuse.Width;
        var h = Diffuse.Height;

        MinorAxis = Math.Min(w, h);

        for (var x = 0; x < w; x++) {
            for (var y = 0; y < h; y++) {
                if (Diffuse[x, y].A != 0) {
                    foreach (var direction in Directions) {
                        var cX = x + direction.X;
                        var cY = y + direction.Y;

                        if (cX < 0 || cX >= w || cY < 0 || cY >= h || Diffuse[cX, cY].A == 0) {
                            edges.Add(new(cX, cY));
                            break;
                        }
                    }
                    Opaque++;
                }
            }
        }

        Edges = new QuadTree(edges, Diffuse.Bounds);
        //Edges = new PointList(edges);
        Opaque /= (float)(w * h);
    }

    public void Export(string path)
    {
        Bevel.Save(path);
    }
}