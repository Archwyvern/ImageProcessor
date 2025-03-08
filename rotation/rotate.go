package rotation

import (
	"bufio"
	"fmt"
	"image"
	"image/png"
	"imageprocessor/parallel"
	"imageprocessor/util"
	"os"
	"path/filepath"
	"strings"
)

var SupportedExtension = ".png"

type rotationTask struct {
	target string
	output string
}

func prepareTasks(targetPath string, outputPath string) ([]rotationTask, error) {
	if _, err := os.Stat(targetPath); os.IsNotExist(err) {
		return nil, fmt.Errorf("directory %s does not exist", targetPath)
	}

	info, err := os.Lstat(targetPath)

	if err != nil {
		return nil, err
	}

	if !info.IsDir() {
		return nil, fmt.Errorf("%s is not a directory", targetPath)
	}

	tasks := make([]rotationTask, 0)

	err = filepath.Walk(targetPath, func(path string, info os.FileInfo, err error) error {
		path = strings.ReplaceAll(path, "\\", "/")
		if err != nil {
			return err
		}

		if !info.IsDir() {
			if filepath.Ext(info.Name()) == SupportedExtension {
				local, _ := strings.CutPrefix(path, targetPath)
				out := outputPath + local

				var task = rotationTask{
					target: path,
					output: out,
				}

				tasks = append(tasks, task)
				os.MkdirAll(filepath.Dir(out), os.ModePerm)

			}
		}

		return nil
	})

	if err != nil {
		return nil, err
	}

	return tasks, nil
}

func ScanAndRotate(rotations int, targetPath string, outputPath string) error {
	var tasks, err = prepareTasks(targetPath, outputPath)

	if err != nil {
		return err
	}

	parallel.ForEach(tasks, func(task rotationTask, errs chan<- error) {
		errs <- Rotate(rotations, task.target, task.output)
	})

	return nil
}

func Rotate(rotations int, target string, output string) error {
	if rotations < 1 || rotations > 3 {
		return fmt.Errorf("rotations must be inclusive between 1 and 3")
	}

	targetFile, err := os.Open(target)
	defer func(targetFile *os.File) {
		err := targetFile.Close()
		if err != nil {
			panic(err)
		}
	}(targetFile)

	if err != nil {
		return err
	}

	outputFile, err := os.Create(output)
	defer func(outputFile *os.File) {
		err := outputFile.Close()
		if err != nil {
			panic(err)
		}
	}(outputFile)

	if err != nil {
		return err
	}

	reader := bufio.NewReader(targetFile)
	writer := bufio.NewWriter(outputFile)

	baseimg, err := png.Decode(reader)

	if err != nil {
		return err
	}

	img := util.ConvertToNRGBAImage(baseimg)

	var newImg *image.NRGBA

	if rotations == 2 {
		newImg = image.NewNRGBA(image.Rect(0, 0, img.Rect.Max.X, img.Rect.Max.Y))
	} else {
		newImg = image.NewNRGBA(image.Rect(0, 0, img.Rect.Max.Y, img.Rect.Max.X))
	}

	parallel.For(0, img.Rect.Max.X, func(x int, errs chan<- error) {
		for y := 0; y < img.Rect.Max.Y; y++ {
			var newX, newY int
			switch rotations % 4 { // Handle rotations in multiples of 90 degrees
			case 0:
				newX = x
				newY = y
			case 1:
				newX = img.Rect.Max.Y - 1 - y
				newY = x
			case 2:
				newX = img.Rect.Max.X - 1 - x
				newY = img.Rect.Max.Y - 1 - y
			case 3:
				newX = y
				newY = img.Rect.Max.X - 1 - x
			}

			newImg.Set(newX, newY, img.At(x, y))
		}
	})

	if err := png.Encode(writer, newImg); err != nil {
		return err
	}

	if err := writer.Flush(); err != nil {
		return err
	}

	return nil
}
