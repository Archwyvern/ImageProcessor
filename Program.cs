using Archwyvern.Space2D.ImageProcessor.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console.Cli;
using System;
using System.Diagnostics;
using System.IO;

namespace Archwyvern.Space2D.ImageProcessor;

class Program
{
    static void Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.AddCommand<GenerateNormalMap>("normal-map")
                .WithDescription("Generate a normal map for an image or all images in a directory"); ;
            config.AddCommand<GenerateAlphaBitmap>("alpha-bitmap")
                .WithDescription("Generate an alpha-bitmap for an image or all images in a directory");
        });

#if DEBUG
        var parsed = Array.Empty<string>();

        if (args.Length > 0) {
            parsed = args[0].Split(' ');
        }

        app.Run(parsed);
#else
        app.Run(args);
#endif
    }
}
