BUILD_DIR        ?= build
XCODE_BUILD_DIR  ?= build-xcode
CMAKE_GENERATOR  ?= Unix Makefiles
BUILD_TYPE       ?= Debug     # Release | RelWithDebInfo ...
JOBS             ?= $(shell nproc 2>/dev/null || sysctl -n hw.ncpu)

CMAKE_FLAGS = \
  -G "$(CMAKE_GENERATOR)" \
  -DCMAKE_BUILD_TYPE=$(BUILD_TYPE) \
  -DCMAKE_EXPORT_COMPILE_COMMANDS=ON

configure:
	@mkdir -p "$(BUILD_DIR)"
	@cmake -S . -B "$(BUILD_DIR)" $(CMAKE_FLAGS)
	@ln -sf "$(BUILD_DIR)/compile_commands.json" .
	@ln -sf "./isotope.manifest" "$(BUILD_DIR)"

build: configure
	@cmake --build "$(BUILD_DIR)" -- -j$(JOBS)

build-xcode:
	@mkdir -p "$(XCODE_BUILD_DIR)"
	@cmake -G Xcode -S . -B "$(XCODE_BUILD_DIR)" -DCMAKE_BUILD_TYPE=Debug

open-xcode: build-xcode
	@open build-xcode/isotope.xcodeproj

install: build
	@cmake --install "$(BUILD_DIR)"

clean:
	@rm -rf "$(BUILD_DIR)"
	@rm -f compile_commands.json

help:
	@echo "Targets:"
	@echo "  configure   - configure the project and generate the compilation rules json"
	@echo "  build       - configure and build"
	@echo "  install     - install the built project into your fusion instillation"
	@echo "  build-xcode - configure and build the xcode project for debugging"
	@echo "  open-xcode  - open the built xcode project in xcode"
	@echo "  clean       - remove build directory and prep for a fresh build"

.DEFAULT_GOAL := build
