#!/bin/sh
# Runs the fission test suite a second time against an ASan-instrumented Jolt
# build (see jolt/CMakeLists.txt's ENABLE_ASAN option) to catch
# use-after-free/double-free bugs in the paths the normal suite already
# exercises. Builds the ASan Jolt binary if it isn't present yet (requires
# Emscripten, see jolt/README.md for EMSCRIPTEN_ROOT/EMSDK setup).
set -e

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
JOLT_DIR="$SCRIPT_DIR/../../jolt"
ASAN_DIST="$JOLT_DIR/dist/jolt-physics.asan.wasm-compat.js"

if [ ! -f "$ASAN_DIST" ]; then
    echo "ASan Jolt build not found at $ASAN_DIST, building it now..."
    (cd "$JOLT_DIR" && bun run build:asan)
fi

export JOLT_ASAN_DIST="$ASAN_DIST"
exec bunx vitest run --project fission-asan
