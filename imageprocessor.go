package main

import (
	"imageprocessor/normalmaps"
	"imageprocessor/rotation"
	"log"
	"os"

	"github.com/urfave/cli/v2"
)

func main() {
	var app = &cli.App{
		Name:                 "imageprocessor",
		Usage:                "A tool for processing texture assets",
		EnableBashCompletion: true,
		Commands: []*cli.Command{
			{
				Name:  "normalmap",
				Usage: "Normal map tool namespace",
				Subcommands: []*cli.Command{
					{
						Name:      "scan",
						Usage:     "Scan a directory for current textures and normal maps",
						Action:    normalmaps.CommandScan,
						Args:      true,
						ArgsUsage: "[directory]",
						Flags: []cli.Flag{
							&cli.StringSliceFlag{
								Name:    "exclude",
								Usage:   "List of filename regular expressions for exclusion",
								Aliases: []string{"e"},
							},
							&cli.StringFlag{
								Name:    "suffix",
								Value:   "_n",
								Usage:   "the normal map suffix",
								Aliases: []string{"s"},
							},
						},
					},
					{
						Name:      "generate",
						Action:    normalmaps.CommandGenerate,
						Usage:     "Create a normal map from a texture or every texture recursively",
						Args:      true,
						ArgsUsage: "[directory or file]",
						Flags: []cli.Flag{
							&cli.StringSliceFlag{
								Name:    "exclude",
								Usage:   "List of filename regular expressions for exclusion",
								Aliases: []string{"e"},
							},
							&cli.BoolFlag{
								Name:  "overwrite",
								Value: false,
								Usage: "Overwrite existing normal maps",
							},
							&cli.StringFlag{
								Name:    "suffix",
								Value:   "_n",
								Usage:   "The normal map suffix",
								Aliases: []string{"s"},
							},
							&cli.StringFlag{
								Name:  "override-suffix",
								Value: "_nx",
								Usage: "The normal map suffix",
							},
							&cli.Float64Flag{
								Name:  "bevel-ratio",
								Value: 100,
								Usage: "The percentage of depth to apply the bevel, this is roughly based on the " +
									"number of opaque pixels",
							},
							&cli.Float64Flag{
								Name:  "bevel-height",
								Value: 25,
								Usage: "The percentage of ratio to do weird stuff with how much of the image is " +
									"faded. Less makes the normals appear more on the outside",
							},
							&cli.Float64Flag{
								Name:  "bevel-smooth",
								Value: 50,
								Usage: "The percentage of depth to blur the bevel, e.g. 10% blur of " +
									"50% depth is 5% blur",
							},
							&cli.Float64Flag{
								Name:  "emboss-height",
								Value: 1,
								Usage: "The height percentage of the emboss effect, higher percentage results " +
									"in more vivid colors.",
							},
							&cli.IntFlag{
								Name:  "emboss-smooth",
								Value: 1,
								Usage: "The number of pixels to blur the source image before applying emboss.",
							},
							&cli.Float64Flag{
								Name:  "emboss-denoise",
								Value: 30,
								Usage: "The normal distance and color difference to ignore, reduces emboss on sutble changes.",
							},
						},
					},
					{
						Name:      "shine",
						Action:    normalmaps.CommandShine,
						Usage:     "Create an example texture with light applied. Requires a texture with a normal map.",
						Args:      true,
						ArgsUsage: "[directory or file]",
						Flags: []cli.Flag{
							&cli.StringSliceFlag{
								Name:    "exclude",
								Usage:   "List of filename regular expressions for exclusion",
								Aliases: []string{"e"},
							},
							&cli.StringFlag{
								Name:    "suffix",
								Value:   "_n",
								Usage:   "The normal map suffix",
								Aliases: []string{"s"},
							},
							&cli.Float64SliceFlag{
								Name:  "direction",
								Value: cli.NewFloat64Slice(45),
								Usage: "The direction of the sun from north in degrees, clamped between -180 and 180",
							},
							&cli.Float64SliceFlag{
								Name:  "energy",
								Value: cli.NewFloat64Slice(1),
								Usage: "Sun's energy, essentially a color multiplier, baseline is 1, 0.5 is half, 2 is double",
							},
							&cli.StringSliceFlag{
								Name:  "color",
								Value: cli.NewStringSlice("ffca83"),
								Usage: "Color of the light in 24bit hexadecimal",
							},
							&cli.Float64Flag{
								Name:  "shadow",
								Value: 0.3,
								Usage: "Percentage of base light level, for example 0.3 will darken the image to 30%% color is no light is present",
							},
							&cli.Float64Flag{
								Name:  "reaction",
								Value: 10,
								Usage: "How reactive the texture is to the light, this is affected by shadow, a darker shadow requires a higher reaction",
							},
						},
					},
				},
			},
			{
				Name:      "rotate",
				Action:    rotation.CommandRotate,
				Usage:     "Rotate a texture or every texture recursively",
				Args:      true,
				ArgsUsage: "[num right angles] [directory or file] [output directory or file]",
			},
		},
	}

	if err := app.Run(os.Args); err != nil {
		log.Fatal(err)
	}
}
