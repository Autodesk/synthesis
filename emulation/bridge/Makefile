GOCMD=go
GOBUILD=$(GOCMD) build
GOCLEAN=$(GOCMD) clean
GOTEST=$(GOCMD) test
GOGET=$(GOCMD) get
GOMOD=$(GOCMD) mod
GOTIDY=$(GOMOD) tidy

PROTOC=./gen-protobuf.sh

BINARY_NAME=server

BINARY_PATH=./bin

all: build-protobuf test build
build-protobuf:
	$(PROTOC)
build:
	$(GOBUILD) -v -o $(BINARY_PATH)/$(BINARY_NAME) ./cmd/server
test:
	$(GOTEST) -v ./...
clean:
	$(GOCLEAN)
	rm -rf $(BINARY_PATH)/$(BINARY_NAME)
run:
	$(GOBUILD) -v -o $(BINARY_PATH)/$(BINARY_NAME) ./cmd/server
	./$(BINARY_PATH)/$(BINARY_NAME)
tidy:
	$(GOTIDY)