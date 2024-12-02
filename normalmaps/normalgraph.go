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

func ColorToNormal(c color.NRGBA) vector3.Vector3 {
	// Convert color components from uint8 [0, 255] to float64 [0, 1]
	r := float64(c.R) / 255.0
	g := float64(c.G) / 255.0
	b := float64(c.B) / 255.0

	// Invert the transformations applied in NormalToColor
	normalX := 2 * (r - 0.5)
	normalY := 2 * (0.5 - g)
	normalZ := 1.0 - b

	// Create and return the normal vector
	return vector3.Vector3{
		X: normalX,
		Y: normalY,
		Z: normalZ,
	}
}
