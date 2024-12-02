package util

func NewMatrix[T any](w int, h int) [][]T {
	matrix := make([][]T, w)
	for x := 0; x < w; x++ {
		matrix[x] = make([]T, h)
	}

	return matrix
}

func IndexValueOrDefault[T any](slice []T, index int, defaultValue T) T {
	if len(slice) <= index {
		return defaultValue
	}

	return slice[index]
}
