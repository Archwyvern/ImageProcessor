package normalmaps

import (
	"errors"
	"fmt"
	"imageprocessor/util"
	"os"
	"path/filepath"
	"time"

	"github.com/deeean/go-vector/vector2"
	"github.com/deeean/go-vector/vector3"
	fmtcolor "github.com/fatih/color"
	"github.com/urfave/cli/v2"
)

func CommandScan(ctx *cli.Context) error {
	var dir = ctx.Args().Get(0)

	if len(dir) == 0 {
		return errors.New("a directory is required")
	}

	var results, err = Scan(
		dir,
		ctx.String("suffix"),
		ctx.String("override-suffix"),
		ctx.StringSlice("exclude"),
	)

	if err != nil {
		return err
	}

	var textureCount = 0
	var normalsCount = 0

	for _, result := range results {
		var n = fmtcolor.RedString("Normals=N")
		if len(result.Normals) > 0 {
			n = fmtcolor.GreenString("Normals=Y")
			normalsCount++
		}

		textureCount++

		var line = fmt.Sprintf("[%s]: %s", n, result.Texture)

		fmt.Println(line)
	}

	fmt.Println()
	fmt.Printf("Textures: %d, Normals found %d\n", textureCount, normalsCount)

	return nil
}

func CommandGenerate(ctx *cli.Context) error {
	var path = ctx.Args().Get(0)

	if len(path) == 0 {
		return errors.New("a file or directory is required")
	}

	if _, err := os.Stat(path); os.IsNotExist(err) {
		return fmt.Errorf("directory %s does not exist", path)
	}

	info, err := os.Stat(path)

	if err != nil {
		return err
	}

	var options = GenerateOptions{
		Excludes:           ctx.StringSlice("exclude"),
		Overwrite:          ctx.Bool("overwrite"),
		FileMarker:         ctx.String("suffix"),
		OverrideFileMarker: ctx.String("override-suffix"),
		BevelRatio:         ctx.Float64("bevel-ratio") / 100,
		BevelHeight:        ctx.Float64("bevel-height") / 100,
		BevelSmooth:        ctx.Float64("bevel-smooth") / 100,
		EmbossHeight:       ctx.Float64("emboss-height") / 100,
		EmbossSmooth:       ctx.Int("emboss-smooth"),
		EmbossDenoise:      ctx.Float64("emboss-denoise") / 100,
	}

	if info.IsDir() {
		sw := time.Now().UnixMilli()
		var results, err = ScanAndGenerate(path, options)

		if err != nil {
			return err
		}

		fmt.Println()
		fmt.Printf("Completed %d images in %.2f seconds\n", len(results), float64(time.Now().UnixMilli()-sw)/1000)
	} else {
		var result, err = Generate(ScanResult{Texture: path}, options)

		if err != nil {
			return err
		}

		fmt.Printf("%s -> %s\n", result.Texture, fmtcolor.GreenString(filepath.Base(result.Normals)))
	}

	return nil
}

func CommandShine(ctx *cli.Context) error {
	var path = ctx.Args().Get(0)

	if len(path) == 0 {
		return errors.New("a file or directory is required")
	}

	if _, err := os.Stat(path); os.IsNotExist(err) {
		return fmt.Errorf("directory %s does not exist", path)
	}

	info, err := os.Stat(path)

	if err != nil {
		return err
	}

	var options = ShineOptions{
		Excludes:   ctx.StringSlice("exclude"),
		FileMarker: ctx.String("suffix"),
		Shadow:     ctx.Float64("shadow"),
		Reaction:   ctx.Float64("reaction"),
	}

	directions := ctx.Float64Slice("direction")
	energies := ctx.Float64Slice("energy")
	colors := ctx.StringSlice("color")

	var lightCount = max(len(directions), len(energies), len(colors))

	for i := 0; i < lightCount; i++ {
		c, err := util.ParseHexToColor(util.IndexValueOrDefault(colors, i, "FFFFFF"))

		if err != nil {
			return err
		}

		direction2 := util.DegreesToVector2(util.IndexValueOrDefault(directions, i, 0))

		direction2 = vector2.Vector2{X: direction2.Y, Y: -direction2.X}
		direction3 := vector3.Vector3{X: direction2.X, Y: direction2.Y, Z: 1}

		light := ShineOptionsLight{
			Direction: direction3,
			Energy:    util.IndexValueOrDefault(energies, i, 1),
			ColorF: vector3.Vector3{
				X: float64(c.R) / 255,
				Y: float64(c.G) / 255,
				Z: float64(c.B) / 255,
			},
		}
		options.Lights = append(options.Lights, light)
	}

	if info.IsDir() {
		sw := time.Now().UnixMilli()
		var results, err = ScanAndShine(path, options)

		if err != nil {
			return err
		}

		fmt.Println()
		fmt.Printf("Completed %d images in %.2f seconds\n", len(results), float64(time.Now().UnixMilli()-sw)/1000)
	} else {
		var result, err = Shine(path, options)

		if err != nil {
			return err
		}

		fmt.Printf("%s -> %s\n", result.Texture, fmtcolor.GreenString(filepath.Base(result.Shining)))
	}

	return nil
}
