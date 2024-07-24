using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Convolution;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Archwyvern.Space2D.ImageProcessor;

internal class EmbossNormalMap
{
    public static Vector3[,] GetNormals(Image<RgbaVector> source, float height = 0.05f, int smooth = 1)
    {
        var w = source.Width;
        var h = source.Height;

        var copy = source.Clone(
            ctx => {
                ctx.Grayscale();
                
                if (smooth > 0) {
                    ctx.GaussianBlur(smooth);
                }
            }
        );

        var normals = new Vector3[source.Width, source.Height];

        for (var x = 1; x < w - 1; x++) {
            for (var y = 1; y < h - 1; y++) {
                if (copy[x, y].A == 0) {
                    normals[x, y] = new(0, 0, 0);
                    continue;
                }

                float topLeft = GetIntensity(copy[x - 1, y - 1]);
                float top = GetIntensity(copy[x, y - 1]);
                float topRight = GetIntensity(copy[x + 1, y - 1]);
                float left = GetIntensity(copy[x - 1, y]);
                float right = GetIntensity(copy[x + 1, y]);
                float bottomLeft = GetIntensity(copy[x - 1, y + 1]);
                float bottom = GetIntensity(copy[x, y + 1]);
                float bottomRight = GetIntensity(copy[x + 1, y + 1]);

                // Calculate the gradient (dx, dy) considering 8 directions
                float dx = (topRight + 2 * right + bottomRight) - (topLeft + 2 * left + bottomLeft);
                float dy = (bottomLeft + 2 * bottom + bottomRight) - (topLeft + 2 * top + topRight);

                // Create the normal vector
                Vector2 normal = -new Vector2(dx, dy);
                var a = normal * 10 * height;

                normals[x, y] = new(a.X, a.Y, normal.Length() * 4 * height);
            }
        }

        return normals;
    }

    private static float GetIntensity(RgbaVector color)
    {
        return color.R * color.A;
    }
}