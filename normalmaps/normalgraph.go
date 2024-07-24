package normalmaps

import (
	"image/color"
	"imageprocessor/util"

	"github.com/deeean/go-vector/vector3"
)

var NeutralNormal = color.NRGBA{
	R: 127,
	G: 127,
	B: 255,
	A: 255,
}

func NormalToColor(normal vector3.Vector3) color.NRGBA {
	if normal.Magnitude() == 0 {
		return NeutralNormal
	}

	return color.NRGBA{
		R: uint8(255 * util.Clamp(0.5+normal.X/2, 0, 1)),
		G: uint8(255 * util.Clamp(0.5-normal.Y/2, 0, 1)),
		B: uint8(255 * (1 - normal.Z)),
		A: uint8(255),
	}
}
