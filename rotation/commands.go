package rotation

import (
	"errors"
	"fmt"
	"os"
	"strconv"

	"github.com/urfave/cli/v2"
)

func CommandRotate(ctx *cli.Context) error {
	var rotations, err = strconv.Atoi(ctx.Args().Get(0))

	if err != nil {
		return err
	}

	var target = ctx.Args().Get(1)
	var output = ctx.Args().Get(2)

	if len(target) == 0 {
		return errors.New("target file or directory is required")
	}

	if len(output) == 0 {
		return errors.New("output file or directory is required")
	}

	if _, err := os.Stat(target); os.IsNotExist(err) {
		return fmt.Errorf("directory %s does not exist", target)
	}

	info, err := os.Stat(target)

	if err != nil {
		return err
	}

	if info.IsDir() {
		_, err = os.Stat(output)

		//if !os.IsNotExist(err) {
		//	return fmt.Errorf("output exists")
		//}

		var err = ScanAndRotate(rotations, target, output)

		if err != nil {
			return err
		}
	} else {
		info, err = os.Stat(output)

		if err == nil && info.IsDir() {
			return fmt.Errorf("output is a directory")
		}

		var err = Rotate(rotations, target, output)

		if err != nil {
			return err
		}
	}

	return nil
}
