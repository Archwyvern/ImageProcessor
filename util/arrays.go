package util

func NewMatrix[T any](w int, h int) [][]T {
	matrix := make([][]T, w)
	for x := 0; x < w; x++ {
		matrix[x] = make([]T, h)
	}

	return matrix
}
