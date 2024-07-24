using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Threading.Tasks;

namespace Archwyvern.Space2D.ImageProcessor;

internal class BevelNormalMap
{
    public static readonly ParallelOptions _parallelOptions = new() {

        MaxDegreeOfParallelism = 4
    };

    static BevelNormalMap()
    {
        _parallelOptions = new();

        // Either Windows doesn't like too many threads or my multithreading sucks.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            _parallelOptions.MaxDegreeOfParallelism = 4;
        }
    }

    private static readonly Point[] _directions = [
        new(1, 0),
        new(0, 1),
        new(-1, 0),
        new(0, -1),

        new(1, 1),
        new(-1, -1),
        new(-1, 1),
        new(1, -1)
    ];

    public static Vector3[,] GetNormals(Image<RgbaVector> source, float ratio, float height, float smooth)
    {
        int w = source.Width;
        int h = source.Height;

        var edges = new List<Vector2>();
        var opaque = 0;

        // TODO: There is something wrong with this, without smoothing you see the streaks. I wonder if I was mean't to use polygons or something.
        for (var x = 0; x < w; x++) {
            for (var y = 0; y < h; y++) {
                if (source[x, y].A != 0) {
                    foreach (var direction in _directions) {
                        var cX = x + direction.X;
                        var cY = y + direction.Y;

                        if (cX < 0 || cX >= w || cY < 0 || cY >= h || source[cX, cY].A == 0) {
                            edges.Add(new(cX, cY));
                            break;
                        }
                    }
                    opaque++;
                }
            }
        }

        var edgeTree = new QuadTree(edges, source.Bounds);

        var depth = CalculateDepth(opaque / (float)(w * h), w, h, ratio);

        var normals = new Vector3[w, h];

        Parallel.For(0, h, _parallelOptions, y => {
            for (int x = 0; x < w; x++) {
                var queryPoint = new Point(x, y);

                if (source[x, y].A > 0 && edgeTree.FindNearestPoint(queryPoint, depth, out var edge)) {
                    var direction = edge - queryPoint;

                    // This part I got mainly through trial and error.
                    var scale = Easing.InCirc.Ease(1 - (direction.Length() / depth));
                    var normal = Vector2.Normalize(direction) * (scale + height / 4);
                    var z = Math.Clamp(scale, 0, 1) * height;

                    normals[x, y] = new Vector3(normal.X, normal.Y, z);
                } else {
                    normals[x, y] = Vector3.Zero;
                }
            }
        });

        if (smooth == 0) {
            return normals;
        }

        return ApplyGaussianBlur(normals, w, h, depth * smooth / 3, source);
    }

    public static float CalculateDepth(float density, int width, int height, float coveragePercentage = 0.5f)
    {
        // Coefficients for the quadratic equation: 4d^2 - 2*(width + height)*d + coveragePercentage*density*width*height = 0
        float a = 4f;
        float b = -2f * (width + height);
        float c = coveragePercentage * density * width * height;
        
        // Calculate the discriminant
        float discriminant = b * b - 4f * a * c;
        
        if (discriminant < 0)
        {
            // If the discriminant is negative, there are no real solutions.
            // Returning 0 as an indicator (could also throw an exception).
            return 0;
        }
        
        // Calculate the two possible solutions
        float sqrtDiscriminant = (float)Math.Sqrt(discriminant);
        float d1 = (-b + sqrtDiscriminant) / (2f * a);
        float d2 = (-b - sqrtDiscriminant) / (2f * a);
        
        // Select the feasible solution
        // Bevel depth cannot be larger than half the minimum dimension of the image
        float maxDepth = Math.Min(width, height) / 2f;
        
        float bevelDepth = 0;
        
        if (d1 >= 0 && d1 <= maxDepth)
        {
            bevelDepth = d1;
        }
        else if (d2 >= 0 && d2 <= maxDepth)
        {
            bevelDepth = d2;
        }
        
        return bevelDepth * 2;
    }

    public static Vector3[,] ApplyGaussianBlur(Vector3[,] normals, int w, int h, float sigma, Image<RgbaVector> source)
    {
        // Determine the radius of the kernel
        int radius = (int)Math.Ceiling(3 * sigma);

        // Generate the separable Gaussian kernel
        float[] kernel = CreateGaussianKernel(radius, sigma);

        Vector3[,] temp = new Vector3[w, h];
        Vector3[,] smoothed = new Vector3[w, h];

        // Apply horizontal Gaussian blur
        Parallel.For(0, h, _parallelOptions, y =>
        {
            for (int x = 0; x < w; x++)
            {
                if (source[x, y].A == 0)
                {
                    continue; // Skip pixels with alpha = 0
                }

                Vector3 sum = Vector3.Zero;
                float weightSum = 0.0f;

                for (int k = -radius; k <= radius; k++)
                {
                    int nx = x + k;
                    if (nx >= 0 && nx < w && source[nx, y].A > 0)
                    {
                        float weight = kernel[k + radius];
                        sum += normals[nx, y] * weight;
                        weightSum += weight;
                    }
                }

                if (weightSum > 0)
                {
                    temp[x, y] = sum / weightSum;
                }
                else
                {
                    temp[x, y] = normals[x, y]; // Preserve original value if no valid neighbors
                }
            }
        });

        // Apply vertical Gaussian blur
        Parallel.For(0, w, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, x =>
        {
            for (int y = 0; y < h; y++)
            {
                if (source[x, y].A == 0)
                {
                    continue; // Skip pixels with alpha = 0
                }

                Vector3 sum = Vector3.Zero;
                float weightSum = 0.0f;

                for (int k = -radius; k <= radius; k++)
                {
                    int ny = y + k;
                    if (ny >= 0 && ny < h && source[x, ny].A > 0)
                    {
                        float weight = kernel[k + radius];
                        sum += temp[x, ny] * weight;
                        weightSum += weight;
                    }
                }

                if (weightSum > 0)
                {
                    smoothed[x, y] = sum / weightSum;
                }
                else
                {
                    smoothed[x, y] = normals[x, y]; // Preserve original value if no valid neighbors
                }
            }
        });

        return smoothed;
    }

    private static float[] CreateGaussianKernel(int radius, float sigma)
    {
        int size = 2 * radius + 1;
        float[] kernel = new float[size];
        float sigma2 = sigma * sigma;
        float normalizationFactor = 1.0f / (float)(Math.Sqrt(2.0 * Math.PI) * sigma);

        float sum = 0.0f;
        for (int i = -radius; i <= radius; i++)
        {
            float exponent = -(i * i) / (2 * sigma2);
            float value = normalizationFactor * (float)Math.Exp(exponent);
            kernel[i + radius] = value;
            sum += value;
        }

        // Normalize the kernel
        for (int i = 0; i < size; i++)
        {
            kernel[i] /= sum;
        }

        return kernel;
    }
}