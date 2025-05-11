package normalmaps

import (
	"image/color"

	"github.com/deeean/go-vector/vector3"
)

var Neutral = vector3.Vector3{
	X: 0,
	Y: 0,
	Z: 1,
}
var NeutralColor = color.NRGBA{
	R: 127,
	G: 127,
	B: 255,
	A: 255,
}
var Transparent = color.NRGBA{
	R: 127,
	G: 127,
	B: 255,
	A: 0,
}

func NormalToColor(normal vector3.Vector3) color.NRGBA {
	if normal.Magnitude() == 0 {
		return Transparent
	}

	r := uint8((0.5 + 0.5*normal.X) * 255)
	g := uint8((0.5 - 0.5*normal.Y) * 255)
	// Z is already [0..1] for a normalized tangent-space normal:
	b := uint8(normal.Z * 255)

	return color.NRGBA{R: r, G: g, B: b, A: 255}
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
