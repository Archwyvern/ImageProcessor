package util

import (
	"image"
	"image/draw"
)

func ConvertToNRGBAImage(img image.Image) *image.NRGBA {
	if nrgba, ok := img.(*image.NRGBA); ok {
		return nrgba
	}

	bounds := img.Bounds()
	nrgbaImage := image.NewNRGBA(bounds)

	draw.Draw(nrgbaImage, bounds, img, image.Point{}, draw.Over)
	return nrgbaImage
}
