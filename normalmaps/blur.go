package normalmaps

import (
	"imageprocessor/parallel"
	"imageprocessor/util"
	"math"

	"github.com/deeean/go-vector/vector3"
)

func blur(normals [][]vector3.Vector3, bitmap [][]bool, w int, h int, sigma float64) [][]vector3.Vector3 {
	radius := int(math.Ceil(3 * sigma))
	kernel := createBlurKernel(radius, sigma)

	temp := util.NewMatrix[vector3.Vector3](w, h)
	smoothed := util.NewMatrix[vector3.Vector3](w, h)

	parallel.For(0, w, func(x int, errs chan<- error) {
		for y := 0; y < h; y++ {
			if !bitmap[x][y] {
				continue
			}

			sum := vector3.Vector3{}
			weightSum := 0.0

			for k := -radius; k <= radius; k++ {
				nx := x + k
				if nx >= 0 && nx < w && bitmap[nx][y] {
					weight := kernel[k+radius]
					sum = *sum.Add(normals[nx][y].MulScalar(weight))
					weightSum += weight
				}
			}

			if weightSum > 0 {
				temp[x][y] = *sum.DivScalar(weightSum)
			} else {
				temp[x][y] = normals[x][y]
			}
		}
		errs <- nil
	})

	parallel.For(0, w, func(x int, errs chan<- error) {
		for y := 0; y < h; y++ {
			if !bitmap[x][y] {
				continue
			}

			sum := vector3.Vector3{}
			weightSum := 0.0

			for k := -radius; k <= radius; k++ {
				ny := y + k
				if ny >= 0 && ny < h && bitmap[x][ny] {
					weight := kernel[k+radius]
					sum = *sum.Add(temp[x][ny].MulScalar(weight))
					weightSum += weight
				}
			}

			if weightSum > 0 {
				smoothed[x][y] = *sum.DivScalar(weightSum)
			} else {
				smoothed[x][y] = normals[x][y]
			}
		}
		errs <- nil
	})

	return smoothed
}

func createBlurKernel(radius int, sigma float64) []float64 {
	size := 2*radius + 1
	kernel := make([]float64, 2*radius+1)
	sigma2 := sigma * sigma

	normalizationFactor := 1 / (math.Sqrt(2*math.Pi) * sigma)
	sum := 0.0

	for i := -radius; i <= radius; i++ {
		exponent := float64(-(i * i)) / (2 * sigma2)
		value := normalizationFactor * math.Exp(exponent)
		kernel[i+radius] = value
		sum += value
	}

	for i := 0; i < size; i++ {
		kernel[i] /= sum
	}

	return kernel
}
