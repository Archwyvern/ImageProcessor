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

func getRadialNormals(img image.NRGBA) [][]vector3.Vector3 {
	w := img.Bounds().Size().X
	h := img.Bounds().Size().Y

	center := vector2.New(float64(w)/2, float64(h)/2)
	maxDistance := center.Distance(vector2.New(0, 0))
	normals := util.NewMatrix[vector3.Vector3](w, h)

	bitmap := util.NewMatrix[bool](w, h)

	parallel.For(0, w, func(x int, errs chan<- error) {
		for y := 0; y < h; y++ {
			point := vector2.New(float64(x), float64(y))
			alpha := img.At(x, y).(color.NRGBA).A
			bitmap[x][y] = alpha > 0

			if alpha > 0 {
				relativeToCenter := point.Sub(center)
				distance := relativeToCenter.Magnitude()
				_ = distance

				normal2 := relativeToCenter.Normalize()

				//var z = util.EaseOutCirc(distance / maxDistance)

				normal := vector3.New(normal2.X, normal2.Y, 0)
				Neutralize(normal, 1-distance/maxDistance)

				normals[x][y] = *normal
				/*
					if edge != nil {
						pV := p2v(point)
						eV := p2v(edge)

						if pV.Distance(&eV) > depth {
							continue
						}

						direction := eV.Sub(&pV)

						scale := util.EaseInCirc(1 - direction.Magnitude()/depth)
						normal := direction.Normalize().MulScalar(scale + height/4)
						z := util.Clamp(scale, 0, 1) * height

						normals[x][y] = vector3.Vector3{
							X: normal.X,
							Y: normal.Y,
							Z: z,
						}

						continue
					}
				*/
			}
		}

		errs <- nil
	})

	return normals
}

func Neutralize(v *vector3.Vector3, ratio float64) {
	// clamp ratio to [0,1]
	if ratio < 0 {
		ratio = 0
	} else if ratio > 1 {
		ratio = 1
	}

	// our neutral target is the up-axis (0,0,1)
	const ux, uy, uz = 0.0, 0.0, 1.0

	// lerp v toward (0,0,1)
	v.X = v.X*(1-ratio) + ux*ratio
	v.Y = v.Y*(1-ratio) + uy*ratio
	v.Z = v.Z*(1-ratio) + uz*ratio

	// re-normalize
	len := Hypot3(v.X, v.Y, v.Z)
	if len > 0 {
		inv := 1.0 / len
		v.X *= inv
		v.Y *= inv
		v.Z *= inv
	}
}

func Hypot3(p, q, r float64) float64 {
	// work with absolute values
	p = math.Abs(p)
	q = math.Abs(q)
	r = math.Abs(r)

	// find the largest component
	max := p
	if q > max {
		max = q
	}
	if r > max {
		max = r
	}

	// if all are zero, return 0
	if max == 0 {
		return 0
	}

	// scale down, compute, then scale back up
	p /= max
	q /= max
	r /= max
	return max * math.Sqrt(p*p+q*q+r*r)
}
