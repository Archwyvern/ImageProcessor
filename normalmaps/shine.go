package normalmaps

import (
	"bufio"
	"errors"
	"fmt"
	"image/color"
	"image/png"
	"imageprocessor/parallel"
	"imageprocessor/util"
	"log"
	"os"
	"path/filepath"
	"time"

	"github.com/deeean/go-vector/vector3"
)

type ShineOptions struct {
	Excludes   []string
	FileMarker string
	Shadow     float64
	Reaction   float64
	Lights     []ShineOptionsLight
}

type ShineOptionsLight struct {
	Direction vector3.Vector3
	Energy    float64
	ColorF    vector3.Vector3
}

type ShineResult struct {
	Texture     string
	Shining     string
	ElapsedTime float64
}

func ScanAndShine(dir string, options ShineOptions) ([]ShineResult, error) {
	var scanResults, err = Scan(dir, options.FileMarker, options.Excludes)

	if err != nil {
		return nil, err
	}

	var results = make([]ShineResult, len(scanResults))

	forError := parallel.For(0, len(scanResults), func(i int, errs chan<- error) {
		if scanResults[i].Normals == "" {
			errs <- nil
			return
		}

		shineResult, err := Shine(scanResults[i].Texture, options)

		if err != nil {
			errs <- err
			return
		}

		fmt.Printf(
			"%s -> %s (%fs)\n",
			shineResult.Texture,
			filepath.Base(shineResult.Shining),
			shineResult.ElapsedTime,
		)

		results[i] = *shineResult
		errs <- nil
	})

	if forError.HasErrors {
		for _, err = range forError.Errors {
			log.Println(err)
		}

		return nil, errors.New("generation occurred with errors")
	}

	return results, nil
}

func Shine(file string, options ShineOptions) (*ShineResult, error) {
	normals, valid := ResolveSuffixedFilePath(file, options.FileMarker)

	if !valid {
		return nil, fmt.Errorf("the provided file is a normal map: %s", file)
	}

	shining, valid := ResolveSuffixedFilePath(file, "shining")

	if !valid {
		return nil, fmt.Errorf("the provided file is a shining: %s", file)
	}

	textureFile, err := os.Open(file)

	if err != nil {
		return nil, err
	}

	normalsFile, err := os.Open(normals)

	if err != nil {
		return nil, err
	}

	shiningFile, err := os.Create(shining)

	if err != nil {
		return nil, err
	}

	textureReader := bufio.NewReader(textureFile)
	normalsReader := bufio.NewReader(normalsFile)
	shiningWriter := bufio.NewWriter(shiningFile)

	defer textureFile.Close()
	defer normalsFile.Close()
	defer shiningFile.Close()

	sw := time.Now().UnixMicro()
	if err := createShining(textureReader, normalsReader, shiningWriter, options); err != nil {
		return nil, fmt.Errorf("%s, %s", file, err)
	}

	result := &ShineResult{
		Texture:     file,
		Shining:     shining,
		ElapsedTime: float64(time.Now().UnixMicro()-sw) / 1000000,
	}

	return result, nil
}

func createShining(textureReader *bufio.Reader, normalsReader *bufio.Reader, shiningWriter *bufio.Writer, options ShineOptions) error {
	textureImage, err := png.Decode(textureReader)

	if err != nil {
		return err
	}

	normalsImage, err := png.Decode(normalsReader)

	if err != nil {
		return err
	}

	texture := convertToNRGBAImage(textureImage)
	normals := convertToNRGBAImage(normalsImage)

	size := texture.Rect.Max

	if size.X != normals.Rect.Max.X || size.Y != normals.Rect.Max.Y {
		return fmt.Errorf("the dimensions of the texture and normal do not match")
	}

	shadow := options.Shadow
	reaction := options.Reaction

	parallel.For(0, size.X, func(x int, errs chan<- error) {
		for y := 0; y < size.Y; y++ {
			c := texture.At(x, y).(color.NRGBA)
			normal := ColorToNormal(normals.At(x, y).(color.NRGBA))

			dR := float64(c.R) * shadow
			dG := float64(c.G) * shadow
			dB := float64(c.B) * shadow

			lightR := 0.0
			lightG := 0.0
			lightB := 0.0

			for _, light := range options.Lights {
				//PixelRGB = LightRGB * TextureRGB * Energy * NormalDot * Reaction
				dot := max(0, normal.Dot(&light.Direction))

				lightR += dR * light.ColorF.X * light.Energy * dot * reaction
				lightG += dG * light.ColorF.Y * light.Energy * dot * reaction
				lightB += dB * light.ColorF.Z * light.Energy * dot * reaction
			}

			newColor := color.NRGBA{
				R: uint8(util.Clamp(dR+lightR, 0, 255)),
				G: uint8(util.Clamp(dG+lightG, 0, 255)),
				B: uint8(util.Clamp(dB+lightB, 0, 255)),
				A: c.A,
			}

			texture.Set(x, y, newColor)
		}

		errs <- nil
	})

	if err := png.Encode(shiningWriter, texture); err != nil {
		return err
	}

	if err := shiningWriter.Flush(); err != nil {
		return err
	}

	return nil
}
