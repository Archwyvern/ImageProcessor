package util

import (
	"math"

	"github.com/deeean/go-vector/vector2"
)

func Clamp(t float64, min float64, max float64) float64 {
	if t < min {
		return min
	}

	if t > max {
		return max
	}

	return t
}

func EaseInCirc(t float64) float64 {
	return -(math.Sqrt(1-t*t) - 1)
}

func DegreesToVector2(directionInDegrees float64) vector2.Vector2 {
	radians := directionInDegrees * math.Pi / 180.0

	x := float64(math.Cos(radians))
	y := float64(math.Sin(radians))

	return vector2.Vector2{X: x, Y: y}
}
