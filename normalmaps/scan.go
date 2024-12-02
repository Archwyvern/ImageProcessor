package normalmaps

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strings"
)

type ScanResult struct {
	Texture string
	Normals string
}

func Scan(targetPath string, marker string, excludes []string) ([]ScanResult, error) {
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

	results := make([]ScanResult, 0)

	err = filepath.Walk(targetPath, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		if !info.IsDir() {

			for _, exclude := range excludes {
				match, _ := regexp.MatchString(exclude, info.Name())

				if match {
					return nil
				}
			}

			if filepath.Ext(info.Name()) == SupportedExtension {
				if normals, valid := ResolveSuffixedFilePath(path, marker); valid {
					var result = ScanResult{
						Texture: path,
					}

					if _, err := os.Stat(normals); err == nil {
						result.Normals = normals
					}

					results = append(results, result)
				}
			}
		}

		return nil
	})

	if err != nil {
		return nil, err
	}

	return results, nil
}

func ResolveSuffixedFilePath(texture string, marker string) (string, bool) {
	var name, _ = strings.CutSuffix(filepath.Base(texture), SupportedExtension)
	marker = resolveMarker(marker)

	if strings.HasSuffix(name, marker) {
		return "", false
	}

	return fmt.Sprintf("%s/%s%s%s", filepath.Dir(texture), name, marker, SupportedExtension), true
}

func resolveMarker(marker string) string {
	if len(marker) == 0 {
		marker = DefaultFileMarker
	}

	if marker[0] != '_' {
		marker = "_" + marker
	}

	return marker
}
