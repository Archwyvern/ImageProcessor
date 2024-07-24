using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Archwyvern.Space2D.ImageProcessor.Commands;

internal sealed class GenerateNormalMap : Command<GenerateNormalMap.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to source of a single image")]
        [CommandArgument(0, "[Source]")]
        public string Source { get; set; }

        [Description("Path to output of a single image")]
        [CommandArgument(1, "[Output]")]
        public string Output { get; set; }

        [Description("The suffix to add to the output file(s) e.g. ship.png => ship_n.png, ignored if output argument is specified")]
        [CommandOption("-s|--suffix")]
        [DefaultValue("_n")]
        public string Suffix { get; set; }

        [Description("Path to output of a single image")]
        [CommandOption("-d|--source-directory")]
        public string SourceDirectory { get; init; }

        [Description("Recursively search through source-directory")]
        [CommandOption("-r|--recursive")]
        [DefaultValue(false)]
        public bool Recursive { get; init; }

        [CommandOption("-o|--output-directory")]
        public string OutputDirectory { get; init; }

        [Description(
            "Regex for file name filter, by default allows PNG files without the _n suffix, " +
            "or if output-suffix is specified any PNG without that suffix.")]
        [CommandOption("-f|--file-filter")]
        [DefaultValue(@"^.+?(?<!_n)\.png$")]
        public string FileFilter { get; init; }

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
        if (settings.Source != null) {
            var output = settings.Output;

            output ??= Path.Combine(
                Path.GetDirectoryName(settings.Source),
                string.Format(
                    "{0}{1}{2}",
                    Path.GetFileNameWithoutExtension(settings.Source),
                    settings.Suffix,
                    Path.GetExtension(settings.Source)
                )
            );

            ProcessFile(
                settings.Source,
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
}