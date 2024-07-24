package normalmaps

import (
	"bufio"
	"image"
	"image/draw"
	"image/png"
	"imageprocessor/parallel"

	"github.com/deeean/go-vector/vector2"
	"github.com/paulmach/orb"
)

func process(reader *bufio.Reader, writer *bufio.Writer, options GenerateOptions) error {
	baseimg, err := png.Decode(reader)

	if err != nil {
		return err
	}

	img := convertToNRGBAImage(baseimg)

	bNormals := getBevelNormals(*img, options.BevelRatio, options.BevelHeight, options.BevelSmooth)
	eNormals := getEmbossNormals(*img, options.EmbossHeight, options.EmbossSmooth)

	w := img.Bounds().Size().X
	h := img.Bounds().Size().Y

	parallel.For(0, w, func(x int, errs chan<- error) {
		for y := 0; y < h; y++ {
			normal := *bNormals[x][y].Add(&eNormals[x][y])
			img.Set(x, y, NormalToColor(normal))
		}

		errs <- nil
	})

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
