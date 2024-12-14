package normalmaps

import (
	"image"
	"image/color"
	"imageprocessor/parallel"
	"imageprocessor/util"
	"math"
	"sort"

	"github.com/deeean/go-vector/vector3"
	"github.com/paulmach/orb"
	"github.com/paulmach/orb/quadtree"
)

func getBevelNormals(img image.NRGBA, ratio float64, height float64, smooth float64) [][]vector3.Vector3 {
	edges := make(map[image.Point]bool)
	opaque := 0

	w := img.Bounds().Size().X
	h := img.Bounds().Size().Y

	for x := 0; x < w; x++ {
		for y := 0; y < h; y++ {
			if img.At(x, y).(color.NRGBA).A == 0 {
				continue
			}

			for _, direction := range directions {
				cX := x + direction.X
				cY := y + direction.Y

				if cX < 0 || cX >= w || cY < 0 || cY >= h || img.At(cX, cY).(color.NRGBA).A == 0 {
					edges[image.Point{X: cX, Y: cY}] = true
				}
			}

			opaque++
		}
	}

	var edgePoints []orb.Point
	for p := range edges {
		edgePoints = append(edgePoints, orb.Point{float64(p.X), float64(p.Y)})
	}
	// Sort edgePoints based on some criteria, for example, by X then Y.
	sort.Slice(edgePoints, func(i, j int) bool {
		if edgePoints[i].X() == edgePoints[j].X() {
			return edgePoints[i].Y() < edgePoints[j].Y()
		}
		return edgePoints[i].X() < edgePoints[j].X()
	})

	tree := quadtree.New(orb.Bound{
		Min: orb.Point{
			-1,
			-1,
		},
		Max: orb.Point{
			float64(w) + 1,
			float64(h) + 1,
		},
	})

	for _, p := range edgePoints {
		err := tree.Add(orb.Point{
			float64(p.X()),
			float64(p.Y()),
		})

		if err != nil {
			panic(err)
		}
	}

	normals := util.NewMatrix[vector3.Vector3](w, h)
	depth := calculateDepth(float64(opaque)/float64(w*h), w, h, ratio)

	bitmap := util.NewMatrix[bool](w, h)

	parallel.For(0, w, func(x int, errs chan<- error) {
		for y := 0; y < h; y++ {
			point := orb.Point{float64(x), float64(y)}
			alpha := img.At(x, y).(color.NRGBA).A
			bitmap[x][y] = alpha > 0

			if alpha > 0 {
				edge := tree.Find(point)

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
			}

			normals[x][y] = vector3.Vector3{}
		}

		errs <- nil
	})

	return blur(normals, bitmap, w, h, depth*smooth/3)
}

func calculateDepth(density float64, w int, h int, ratio float64) float64 {
	var a float64 = 4
	var b = -2 * float64(w+h)
	var c = ratio * density * float64(w*h)

	discriminant := b*b - 4*a*c

	if discriminant < 0 {
		return 0
	}

	sqrtDiscriminant := math.Sqrt(discriminant)
	d1 := (-b + sqrtDiscriminant) / (2 * a)
	d2 := (-b - sqrtDiscriminant) / (2 * a)

	maxDepth := float64(min(w, h)) / 2

	var bevelDepth float64 = 0

	if d1 >= 0 && d1 <= maxDepth {
		bevelDepth = d1
	} else if d2 >= 0 && d2 <= maxDepth {
		bevelDepth = d2
	}

	return bevelDepth * 2
}
