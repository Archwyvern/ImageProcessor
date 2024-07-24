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
    private Image<RgbaVector> Source;
    private float Height;

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

    public EmbossNormalMap(Image<RgbaVector> source, float height = 0.05f, float smooth = 1)
    {
        Source = source.Clone(
            ctx => {
                ctx.Grayscale();
                ctx.GaussianBlur(smooth);
            }
        );
        Height = height;
    }

    public Vector3 GetNormal(Point point) => GetNormal(point.X, point.Y);
    public Vector3 GetNormal(int x, int y)
    {
        if (Source[x, y].A == 0) {
            return new(0, 0, 1);
        }

        float topLeft = GetIntensity(Source[x - 1, y - 1]);
        float top = GetIntensity(Source[x, y - 1]);
        float topRight = GetIntensity(Source[x + 1, y - 1]);
        float left = GetIntensity(Source[x - 1, y]);
        float right = GetIntensity(Source[x + 1, y]);
        float bottomLeft = GetIntensity(Source[x - 1, y + 1]);
        float bottom = GetIntensity(Source[x, y + 1]);
        float bottomRight = GetIntensity(Source[x + 1, y + 1]);

        // Calculate the gradient (dx, dy) considering 8 directions
        float dx = (topRight + 2 * right + bottomRight) - (topLeft + 2 * left + bottomLeft);
        float dy = (bottomLeft + 2 * bottom + bottomRight) - (topLeft + 2 * top + topRight);

        // Create the normal vector
        Vector2 normal = -new Vector2(dx, dy);

        return new(0, 0, 1);
    }

    public static Image<RgbaVector> Apply(Image<RgbaVector> source, float height = 0.05f, float smooth = 1)
    {
        int w = source.Width;
        int h = source.Height;

        // Convert image to grayscale

        var grayscale = source.Clone();
        grayscale.Mutate(ctx => ctx.Grayscale());

        grayscale.Mutate(ctx => ctx.GaussianBlur(smooth));

        // Create a new image to hold the embossed result
        var result = new Image<RgbaVector>(w, h);

        // Iterate through each pixel in the image
        for (int y = 1; y < h - 1; y++)
        {
            for (int x = 1; x < w - 1; x++)
            {
                if (grayscale[x, y].A == 0) {
                    result[x, y] = NormalGraph.Neutral;
                    continue;
                }

                // Get the intensity values of the neighboring pixels
                float topLeft = GetIntensity(grayscale[x - 1, y - 1]);
                float top = GetIntensity(grayscale[x, y - 1]);
                float topRight = GetIntensity(grayscale[x + 1, y - 1]);
                float left = GetIntensity(grayscale[x - 1, y]);
                float right = GetIntensity(grayscale[x + 1, y]);
                float bottomLeft = GetIntensity(grayscale[x - 1, y + 1]);
                float bottom = GetIntensity(grayscale[x, y + 1]);
                float bottomRight = GetIntensity(grayscale[x + 1, y + 1]);

                // Calculate the gradient (dx, dy) considering 8 directions
                float dx = (topRight + 2 * right + bottomRight) - (topLeft + 2 * left + bottomLeft);
                float dy = (bottomLeft + 2 * bottom + bottomRight) - (topLeft + 2 * top + topRight);

                // Create the normal vector
                Vector2 normal = -new Vector2(dx, dy);

                // Get the color from the normal graph
                RgbaVector color = NormalGraph.GetColor(normal * 10 * height, normal.Length() * 4 * height);

                // Set the color in the result image
                result[x, y] = color;
            }
        }

        // Handle edges by copying over from the source image
        for (int x = 0; x < w; x++)
        {
            result[x, 0] = NormalGraph.Neutral;
            result[x, h - 1] = NormalGraph.Neutral;
        }

        for (int y = 0; y < h; y++)
        {
            result[0, y] = NormalGraph.Neutral;
            result[w - 1, y] = NormalGraph.Neutral;
        }

        for (int y = 0; y < result.Height; y++) {
            for (int x = 0; x < result.Width; x++) {
                if (source[x, y].A == 0) {
                    result[x, y] = NormalGraph.Neutral;
                }
            }
        }

        return result;
    }

    private static float GetIntensity(RgbaVector color)
    {
        return color.R * color.A;
    }
}