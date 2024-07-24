package util

import "math"

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
