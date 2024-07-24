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
                .WithDescription("Generate a normal map for an image or all images in a directory")
                .WithExample("normal-map", "ship.php")
                .WithExample("normal-map", "ship.php", "ship_n.png")
                .WithExample("normal-map", "ship.php", "-suffix=_n")
                .WithExample("normal-map", "-d=./source", "-o=./output");
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
