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

internal sealed class GenerateNormalMap : Command<GenerateNormalMap.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to source of a single image or source directory")]
        [CommandArgument(0, "<Source>")]
        public string Source { get; set; }

        [Description("The suffix to add to the output file(s) e.g. ship.png => ship_n.png, ignored if output argument is specified")]
        [CommandOption("-s|--suffix")]
        [DefaultValue("_n")]
        public string Suffix { get; set; }

        [Description("Recursively search through source-directory")]
        [CommandOption("-r|--recursive")]
        [DefaultValue(false)]
        public bool Recursive { get; init; }

        [Description("Regex for file names to include")]
        [CommandOption("-i|--include")]
        public string[] Include { get; init; }

        [Description("Regex for file names to exclude")]
        [CommandOption("-e|--exclude")]
        public string[] Exclude { get; init; }

        [Description("Output directory")]
        [CommandOption("-o|--output")]
        public string Output { get; init; }

        [Description("The percentage of depth to apply the bevel, this is roughly based on the number of opaque pixels.")]
        [CommandOption("--bevel-ratio")]
        [DefaultValue(100f)]
        public float BevelRatio { get; init; }

        [Description("The percentage of ratio to do weird stuff with how much of the image is faded. Less makes the normals appear more on the outside.")]
        [CommandOption("--bevel-height")]
        [DefaultValue(25f)]
        public float BevelHeight { get; init; }

        [Description("The percentage of depth to blur the bevel, e.g. 10% blur of 50% depth is 5% blur")]
        [CommandOption("--bevel-smooth")]
        [DefaultValue(50f)]
        public float BevelSmooth { get; init; }

        [Description("The height percentage of the emboss effect, higher percentage results in more vivid colors.")]
        [CommandOption("--emboss-height")]
        [DefaultValue(5f)]
        public float EmbossHeight { get; init; }

        [Description("The number of pixels to blur the source image before applying emboss.")]
        [CommandOption("--emboss-smooth")]
        [DefaultValue(1)]
        public int EmbossSmooth { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var include = new List<string>(settings.Include ?? []);
        var exclude = new List<string>(settings.Exclude ?? []);

        include.Add("\\.png$");
        exclude.Add($"{settings.Suffix}\\.png$");

        var sourceFiles = new List<string>();
        var isSourceDir = false;

        if (Directory.Exists(settings.Source)) {
            GetFiles(settings.Source, sourceFiles, include, exclude, settings.Recursive);
            isSourceDir = true;
        } else {
            sourceFiles.Add(settings.Source);
        }

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

            if (isSourceDir && settings.Output != null) {
                output = settings.Output + output[settings.Source.Length..];

                Directory.CreateDirectory(Path.GetDirectoryName(output));
            }

            ProcessFile(
                file,
                output,
                settings.BevelRatio / 100,
                settings.BevelHeight / 100,
                settings.BevelSmooth / 100,
                settings.EmbossHeight / 100,
                settings.EmbossSmooth
            );
        }

        AnsiConsole.MarkupLine($"Bevel Ratio:   [blue]{settings.BevelRatio}%[/]");
        AnsiConsole.MarkupLine($"Bevel Height:  [blue]{settings.BevelHeight}%[/]");
        AnsiConsole.MarkupLine($"Bevel Smooth:  [blue]{settings.BevelSmooth}[/]");
        AnsiConsole.MarkupLine($"Emboss Height: [blue]{settings.EmbossHeight}%[/]");
        AnsiConsole.MarkupLine($"Emboss Smooth: [blue]{settings.EmbossSmooth}[/]");

        return 0;
    }

    private static void ProcessDirectory(
        string source,
        float bevelRatio,
        float bevelHeight,
        float bevelSmooth,
        float embossHeight,
        int embossSmooth
    ) {
        
    }

    private static void ProcessFile(
        string source,
        string output,
        float bevelRatio,
        float bevelHeight,
        float bevelSmooth,
        float embossHeight,
        int embossSmooth
    ) {
        var stopwatch = Stopwatch.StartNew();
        
        var diffuse = Image.Load<RgbaVector>(Path.GetFullPath(source));
        var image = diffuse.Clone();
        
        var bNormals = BevelNormalMap.GetNormals(image, bevelRatio, bevelHeight, bevelSmooth);
        var eNormals = EmbossNormalMap.GetNormals(image, embossHeight, embossSmooth);

        var w = image.Width;
        var h = image.Height;

        for (var x = 0; x < w; x++) {
            for (var y = 0; y < h; y++) {
                image[x, y] = NormalGraph.GetColor(bNormals[x, y] + eNormals[x, y]);
            }
        }

        var elapsed = Math.Round(stopwatch.Elapsed.TotalSeconds, 4);

        AnsiConsole.MarkupLine($"[blue]{source}[/] -> [green]{output}[/] : {elapsed}s");

        image.Save(Path.GetFullPath(output));
    }

    private static void GetFiles(
        string directory,
        List<string> files,
        List<string> include,
        List<string> exclude,
        bool recursive = false
    ) {
        foreach (var file in Directory.GetFiles(directory)) {
            var allow = true;

            foreach (var inc in include) {
                var includeR = new Regex(inc);

                if (!includeR.IsMatch(file)) {
                    allow = false;
                    break;
                }
            }

            if (!allow) {
                continue;
            }

            foreach (var exc in exclude) {
                var excludeR = new Regex(exc);

                if (excludeR.IsMatch(file)) {
                    allow = false;
                    break;
                }
            }

            if (!allow) {
                continue;
            }

            files.Add(file);
        }

        if (recursive) {
            var directories = Directory.GetDirectories(directory);

            foreach (var dir in Directory.GetDirectories(directory)) {
                GetFiles(dir, files, include, exclude, true);
            }
        }
    }
}
