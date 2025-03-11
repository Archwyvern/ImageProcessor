package normalmaps

import (
	"image"
	"image/color"
	"imageprocessor/parallel"
	"imageprocessor/util"
	"math"

	"github.com/deeean/go-vector/vector2"
	"github.com/deeean/go-vector/vector3"
)

func getEmbossNormals(img image.NRGBA, height float64, smooth int, denoise float64) [][]vector3.Vector3 {
	w := img.Bounds().Dx()
	h := img.Bounds().Dy()

	grayscale := util.NewMatrix[float64](w, h)
	normals := util.NewMatrix[vector3.Vector3](w, h)
	bitmap := util.NewMatrix[bool](w, h)

	parallel.For(0, w, func(x int, errs chan<- error) {
		for y := 0; y < h; y++ {
			c := img.At(x, y).(color.NRGBA)
			grayscale[x][y] = (float64(c.R)/255*0.2126 + float64(c.G)/255*0.7152 + float64(c.B)/255) * float64(c.A) / 255
			bitmap[x][y] = c.A > 0
		}
		errs <- nil
	})

	parallel.For(0, w, func(x int, errs chan<- error) {
		for y := 0; y < h; y++ {
			if !bitmap[x][y] {
				normals[x][y] = vector3.Vector3{}
				continue
			}

			center := grayscale[x][y]

			topLeft := getAdjacentLevel(center, grayscale, x-1, y-1, w, h, denoise)
			top := getAdjacentLevel(center, grayscale, x, y-1, w, h, denoise)
			topRight := getAdjacentLevel(center, grayscale, x+1, y-1, w, h, denoise)
			left := getAdjacentLevel(center, grayscale, x-1, y, w, h, denoise)
			right := getAdjacentLevel(center, grayscale, x+1, y, w, h, denoise)
			bottomLeft := getAdjacentLevel(center, grayscale, x-1, y+1, w, h, denoise)
			bottom := getAdjacentLevel(center, grayscale, x, y+1, w, h, denoise)
			bottomRight := getAdjacentLevel(center, grayscale, x+1, y+1, w, h, denoise)

			dx := (topRight + 2*right + bottomRight) - (topLeft + 2*left + bottomLeft)
			dy := (bottomLeft + 2*bottom + bottomRight) - (topLeft + 2*top + topRight)

			normal := vector2.Vector2{X: -dx, Y: -dy}

			a := normal.MulScalar(10 * height)

			normals[x][y] = vector3.Vector3{
				X: a.X,
				Y: a.Y,
				Z: normal.Magnitude() * 4 * height,
			}
		}
		errs <- nil
	})

	return blur(normals, bitmap, w, h, float64(smooth))
}

func getAdjacentLevel(center float64, matrix [][]float64, x int, y int, w int, h int, denoise float64) float64 {
	skip := (x < 0 || y < 0 || x >= w || y >= h) || math.Abs(center-matrix[x][y]) < denoise

	if skip {
		return center
	}

	return matrix[x][y]
}
