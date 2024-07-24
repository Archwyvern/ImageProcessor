set GOARCH=amd64
set GOOS=linux

go build -o=bin/imageprocessor.exe imageprocessor.go

set GOARCH=amd64
set GOOS=windows

go build -o=bin/imageprocessor.exe imageprocessor.go