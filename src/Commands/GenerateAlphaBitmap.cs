using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Archwyvern.Space2D.ImageProcessor.Commands;

internal sealed class GenerateAlphaBitmap : Command<GenerateAlphaBitmap.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to source directory")]
        [CommandArgument(0, "<Source>")]
        public string Source { get; set; }

        [Description("Output directory")]
        [CommandArgument(0, "<Output>")]
        public string Output { get; init; }

        [Description("The suffix to add to the output file(s) e.g. ship.png => ship_n.png, ignored if output argument is specified")]
        [CommandOption("-s|--suffix")]
        [DefaultValue("_b")]
        public string Suffix { get; set; }

        [Description("Recursively search through source-directory")]
        [CommandOption("-r|--recursive")]
        [DefaultValue(false)]
        public bool Recursive { get; init; }

        [Description("Highlight transparency instead, blue = 1 alpha, Red = more transpanent, Green = less transparent but not opaque")]
        [CommandOption("--highlight")]
        [DefaultValue(false)]
        public bool Highlight { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var sourceFiles = new List<string>();

        if (!Directory.Exists(settings.Source)) {
            throw new Exception($"{settings.Source} does not exist");
        }

        GetFiles(settings.Source, sourceFiles, settings.Recursive);

        foreach (var file in sourceFiles) {
            var output = Path.Combine(
                Path.GetDirectoryName(file),
                string.Format(
                    "{0}{1}{2}",
                    Path.GetFileNameWithoutExtension(file),
                    settings.Suffix,
                    Path.GetExtension(file)
                )
            );

            output = settings.Output + output[settings.Source.Length..];

            Directory.CreateDirectory(Path.GetDirectoryName(output));

            try {
                ProcessFile(
                    file,
                    output,
                    settings.Highlight
                );
            } catch (Exception exception) {
                AnsiConsole.MarkupLine($"[red]{exception.Message}[/]");

                return 1;
            }
        }

        return 0;
    }

    private static void ProcessFile(
        string source,
        string output,
        bool highlight
    ) {
        var stopwatch = Stopwatch.StartNew();
        var image = Image.Load<RgbaVector>(Path.GetFullPath(source));

        var w = image.Width;
        var h = image.Height;

        for (var x = 0; x < w; x++) {
            for (var y = 0; y < h; y++) {
                if (highlight) {
                    if (image[x, y].A == 0) {
                        image[x, y] = new RgbaVector(0, 0, 0, 1);
                    } else if (image[x, y].A == 1) {
                        image[x, y] = new RgbaVector(0, 0, 1, 1);
                    } else {
                        image[x, y] = new RgbaVector(
                            1 - image[x, y].A,
                            image[x, y].A,
                            0,
                            1
                        );
                    }
                } else {
                    image[x, y] = image[x, y].A > 0 ? new RgbaVector(1, 1, 1, 1) : new RgbaVector(0, 0, 0, 1);
                }
            }
        }

        var elapsed = Math.Round(stopwatch.Elapsed.TotalSeconds, 4);

        AnsiConsole.MarkupLine($"[blue]{source}[/] -> [green]{output}[/]");

        image.Save(Path.GetFullPath(output));
    }

    private static void GetFiles(
        string directory,
        List<string> files,
        bool recursive = false
    ) {
        foreach (var file in Directory.GetFiles(directory)) {
            if (Path.GetExtension(file) != ".png") {
                continue;
            }
            files.Add(file);
        }

        if (recursive) {
            var directories = Directory.GetDirectories(directory);

            foreach (var dir in Directory.GetDirectories(directory)) {
                GetFiles(dir, files, true);
            }
        }
    }
}
