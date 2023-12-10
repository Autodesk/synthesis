# Fission - Web based Synthesis
## Quick Start
### Dependencies
1. Protobuf Compiler -> Please see [Original Engine README](../engine/README.md) for installation instructions.
2. Emscripten/EmSDK -> Please follow the [Emscripten README](https://github.com/emscripten-core/emscripten) for installation instructions.
3. CMake -> Google it
4. Make -> Google it (and yes, this includes Windows devs as well)
5. NPM and NodeJs -> Google it

### Setup
Make sure you add the necessary emscripten executables to your path. In addition to your path variable, create an additional environment variable called `EMSDK` that is pointed to the root directory of the Emscripten repository. This will be needed for the CMake target and emscripten header files.

### Engine/Emscripten
Follow the [New Engine README](engine/README.md) for compiling WASM.

### React App
Follow the [Fission App README](synthesis-app/README.md) for instructions on running the react app.