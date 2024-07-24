package parallel

import (
	"runtime"
	"sync"
)

type ForError struct {
	HasErrors bool
	NumErrors int
	Errors    []error
}

type ForFunc func(i int, errs chan<- error)

func For(i int, until int, fn ForFunc) ForError {
	workers := runtime.NumCPU()

	errCh := make(chan error, workers)

	jobs := make(chan int, until-i)
	go func() {
		for j := i; j < until; j++ {
			jobs <- j
		}
		close(jobs)
	}()

	var wg sync.WaitGroup
	for w := 0; w < workers; w++ {
		wg.Add(1)
		go func() {
			defer wg.Done()
			for job := range jobs {
				fn(job, errCh)
			}
		}()
	}

	go func() {
		wg.Wait()
		close(errCh)
	}()

	var forError ForError
	for err := range errCh {
		if err != nil {
			forError.Errors = append(forError.Errors, err)
			forError.HasErrors = true
			forError.NumErrors++
		}
	}
	return forError
}

type ForEachError struct {
	HasErrors bool
	NumErrors int
	Errors    []error
}

type ForEachFunc[T any] func(t T, errs chan<- error)

func ForEach[T any](items []T, fn ForEachFunc[T]) ForEachError {
	workers := runtime.NumCPU()
	errCh := make(chan error, workers)
	jobs := make(chan T, len(items))

	go func() {
		for _, item := range items {
			jobs <- item
		}
		close(jobs)
	}()

	var wg sync.WaitGroup
	for w := 0; w < workers; w++ {
		wg.Add(1)
		go func() {
			defer wg.Done()
			for job := range jobs {
				fn(job, errCh)
			}
		}()
	}

	go func() {
		wg.Wait()
		close(errCh)
	}()

	forError := ForEachError{}
	for err := range errCh {
		if err != nil {
			forError.Errors = append(forError.Errors, err)
			forError.HasErrors = true
			forError.NumErrors++
		}
	}

	return forError
}
