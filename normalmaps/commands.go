package normalmaps

import (
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"time"

	"github.com/fatih/color"
	"github.com/urfave/cli/v2"
)

func CommandScan(ctx *cli.Context) error {
	var dir = ctx.Args().Get(0)

	if len(dir) == 0 {
		return errors.New("A directory is required")
	}

	var results, err = Scan(
		dir,
		ctx.String("suffix"),
		ctx.StringSlice("exclude"),
	)

	if err != nil {
		return err
	}

	var textureCount = 0
	var normalsCount = 0

	for _, result := range results {
		var n = color.RedString("Normals=N")
		if len(result.Normals) > 0 {
			n = color.GreenString("Normals=Y")
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
		return errors.New("A file or directory is required")
	}

	if _, err := os.Stat(path); os.IsNotExist(err) {
		return fmt.Errorf("directory %s does not exist", path)
	}

	info, err := os.Stat(path)

	if err != nil {
		return err
	}

	var options = GenerateOptions{
		Excludes:     ctx.StringSlice("exclude"),
		Overwrite:    ctx.Bool("overwrite"),
		FileMarker:   ctx.String("suffix"),
		BevelRatio:   ctx.Float64("bevel-ratio") / 100,
		BevelHeight:  ctx.Float64("bevel-height") / 100,
		BevelSmooth:  ctx.Float64("bevel-smooth") / 100,
		EmbossHeight: ctx.Float64("emboss-height") / 100,
		EmbossSmooth: ctx.Int("emboss-smooth"),
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
		var result, err = Generate(path, options)

		if err != nil {
			return err
		}

		fmt.Printf("%s -> %s\n", result.Texture, color.GreenString(filepath.Base(result.Normals)))
	}

	return nil
}
