#!/bin/sh

# Run via `bun run test:asan`` from fission/
# Run all of our tests with address sanitization, requires emscripten.

set -e

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
JOLT_DIR="$SCRIPT_DIR/../../jolt"
ASAN_DIST="$JOLT_DIR/dist/jolt-physics.asan.wasm-compat.js"

if [ ! -f "$JOLT_DIR/build.sh" ]; then
    echo "jolt submodule not checked out, run:\n\tgit submodule update --init jolt" >&2
    exit 1
fi

if [ ! -f "$ASAN_DIST" ]; then
    echo "ASan Jolt build not found at $ASAN_DIST, building it now..."
    (cd "$JOLT_DIR" && sh build.sh Debug -DENABLE_ASAN=ON -DBUILD_WASM_COMPAT_ONLY=ON)
    cp "$JOLT_DIR/dist/jolt-physics.debug.wasm-compat.js" "$ASAN_DIST"
    cp "$JOLT_DIR/dist/jolt-physics.debug.wasm-compat.d.ts" "$JOLT_DIR/dist/jolt-physics.asan.wasm-compat.d.ts"
fi

export JOLT_ASAN_DIST="$ASAN_DIST"
exec bunx vitest run --project fission-asan
