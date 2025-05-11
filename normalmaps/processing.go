package normalmaps

import (
	"bufio"
	"image"
	"image/color"
	"image/draw"
	"image/png"
	"imageprocessor/parallel"
	"imageprocessor/util"

	"github.com/deeean/go-vector/vector2"
	"github.com/deeean/go-vector/vector3"
	"github.com/paulmach/orb"
)

func process(reader *bufio.Reader, overrideReader *bufio.Reader, writer *bufio.Writer, options GenerateOptions) error {
	baseimg, err := png.Decode(reader)

	if err != nil {
		return err
	}

	img := convertToNRGBAImage(baseimg)

	rNormals := getRadialNormals(*img)
	bNormals := getBevelNormals(*img, options.BevelRatio, options.BevelHeight, options.BevelSmooth)
	eNormals := getEmbossNormals(*img, options.EmbossSmooth, options.EmbossDenoise)

	w := img.Bounds().Size().X
	h := img.Bounds().Size().Y

	if overrideReader != nil {
		overrideImage, err := png.Decode(overrideReader)

		if err != nil {
			return err
		}

		overrides := convertToNRGBAImage(overrideImage)
		oNormals := util.NewMatrix[vector3.Vector3](w, h)
		oNormalsBitmap := util.NewMatrix[bool](w, h)

		parallel.For(0, w, func(x int, errs chan<- error) {
			for y := range h {
				c := overrides.At(x, y).(color.NRGBA)
				oNormals[x][y] = ColorToNormal(overrides.At(x, y).(color.NRGBA))

				if c.A > 0 {
					oNormalsBitmap[x][y] = true
				}
			}

			errs <- nil
		})

		parallel.For(0, w, func(x int, errs chan<- error) {
			for y := range h {
				if img.At(x, y).(color.NRGBA).A > 0 {
					normal := bNormals[x][y]

					if oNormalsBitmap[x][y] {
						//oNormal := oNormals[x][y]
						//oNormal = *oNormal.MulScalar(0.5)
						//normal = *normal.MulScalar(0.5)
						//normal = *normal.Add(&oNormal)

						normal = oNormals[x][y]
					}

					normal = *normal.Add(&rNormals[x][y]).Add(&eNormals[x][y]).Normalize()
					img.Set(x, y, NormalToColor(normal))
				} else {
					img.Set(x, y, Transparent)
				}
			}

			errs <- nil
		})
	} else {
		parallel.For(0, w, func(x int, errs chan<- error) {
			for y := range h {
				if img.At(x, y).(color.NRGBA).A > 0 {
					normal := *bNormals[x][y].Add(&rNormals[x][y]).Add(&eNormals[x][y]).Normalize()
					img.Set(x, y, NormalToColor(normal))
				} else {
					img.Set(x, y, Transparent)
				}
			}

			errs <- nil
		})
	}

	if err := png.Encode(writer, img); err != nil {
		return err
	}

	if err := writer.Flush(); err != nil {
		return err
	}

	return nil
}

var directions = []image.Point{
	{1, 0},
	{-1, 0},
	{0, 1},
	{0, -1},
	{1, 1},
	{-1, -1},
	{-1, 1},
	{1, -1},
}

func p2v(pointer orb.Pointer) vector2.Vector2 {
	return vector2.Vector2{
		X: pointer.Point().X(),
		Y: pointer.Point().Y(),
	}
}

func convertToNRGBAImage(img image.Image) *image.NRGBA {
	if nrgba, ok := img.(*image.NRGBA); ok {
		return nrgba
	}

	bounds := img.Bounds()
	nrgbaImage := image.NewNRGBA(bounds)

	draw.Draw(nrgbaImage, bounds, img, image.Point{}, draw.Over)
	return nrgbaImage
}
