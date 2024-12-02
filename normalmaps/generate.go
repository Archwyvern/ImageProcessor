package normalmaps

import (
	"bufio"
	"errors"
	"fmt"
	"imageprocessor/parallel"
	"log"
	"os"
	"path/filepath"
	"time"
)

var DefaultFileMarker = "_n"
var SupportedExtension = ".png"

type GenerateOptions struct {
	Excludes     []string
	Overwrite    bool
	FileMarker   string
	BevelRatio   float64
	BevelHeight  float64
	BevelSmooth  float64
	EmbossHeight float64
	EmbossSmooth int
}

type GenerateResult struct {
	Texture     string
	Normals     string
	ElapsedTime float64
}

func ScanAndGenerate(dir string, options GenerateOptions) ([]GenerateResult, error) {
	var scanResults, err = Scan(dir, options.FileMarker, options.Excludes)

	if err != nil {
		return nil, err
	}

	var results = make([]GenerateResult, len(scanResults))

	forError := parallel.For(0, len(scanResults), func(i int, errs chan<- error) {
		if !options.Overwrite && scanResults[i].Normals != "" {
			errs <- nil
			return
		}

		generationResult, err := Generate(scanResults[i].Texture, options)

		if err != nil {
			errs <- err
			return
		}

		fmt.Printf(
			"%s -> %s (%fs)\n",
			generationResult.Texture,
			filepath.Base(generationResult.Normals),
			generationResult.ElapsedTime,
		)

		results[i] = *generationResult
		errs <- nil
	})

	if forError.HasErrors {
		for _, err = range forError.Errors {
			log.Println(err)
		}

		return nil, errors.New("generation occurred with errors")
	}

	return results, nil
}

func Generate(file string, options GenerateOptions) (*GenerateResult, error) {
	normals, valid := ResolveSuffixedFilePath(file, options.FileMarker)

	if !valid {
		return nil, fmt.Errorf("the provided file is already a normal map: %s", file)
	}

	textureFile, err := os.Open(file)

	if err != nil {
		return nil, err
	}

	normalsFile, err := os.Create(normals)

	if err != nil {
		return nil, err
	}

	reader := bufio.NewReader(textureFile)
	writer := bufio.NewWriter(normalsFile)

	sw := time.Now().UnixMicro()
	if err := process(reader, writer, options); err != nil {
		return nil, err
	}

	result := &GenerateResult{
		Texture:     file,
		Normals:     normals,
		ElapsedTime: float64(time.Now().UnixMicro()-sw) / 1000000,
	}

	defer func(textureFile *os.File) {
		err := textureFile.Close()
		if err != nil {
			panic(err)
		}
	}(textureFile)

	defer func(normalsFile *os.File) {
		err := normalsFile.Close()
		if err != nil {
			panic(err)
		}
	}(normalsFile)

	return result, nil
}
