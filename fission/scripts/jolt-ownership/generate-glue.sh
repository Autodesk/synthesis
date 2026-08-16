#!/bin/sh

# Regenerates jolt/JoltJS.idl -> glue.cpp via Emscripten's webidl_binder.py, without a full wasm
# build. webidl_binder.py is a standalone script -- it doesn't need emcc, CMake, or the jolt
# submodule's C++ sources compiled, just the .idl files -- so this is fast (~1s) and safe to run
# on every invocation of the ownership-table generator.
#
# glue.cpp is the actual compiler output the JS/wasm build ships, so parsing it is ground truth
# for return-ownership (COPY vs a function-local `static` scratch vs a bare member reference),
# not a re-derivation of webidl_binder.py's codegen rules from the .idl tags by hand.

set -e

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
JOLT_DIR="$SCRIPT_DIR/../../../jolt"
OUT_DIR="$SCRIPT_DIR/.generated"

if [ ! -f "$JOLT_DIR/JoltJS.idl" ]; then
    echo "jolt submodule not checked out, run:\n\tgit submodule update --init jolt" >&2
    exit 1
fi

if [ -z "$EMSCRIPTEN_ROOT" ]; then
    if [ -d "/opt/homebrew/opt/emscripten/libexec" ]; then
        EMSCRIPTEN_ROOT=/opt/homebrew/opt/emscripten/libexec
    elif [ -n "$EMSDK" ]; then
        EMSCRIPTEN_ROOT=$EMSDK/upstream/emscripten
    else
        echo "Error: set EMSCRIPTEN_ROOT or EMSDK (need webidl_binder.py, no full emscripten build required)" >&2
        exit 1
    fi
fi

WEBIDL_BINDER="$EMSCRIPTEN_ROOT/tools/webidl_binder.py"
if [ ! -f "$WEBIDL_BINDER" ]; then
    echo "Error: webidl_binder.py not found at $WEBIDL_BINDER" >&2
    exit 1
fi

mkdir -p "$OUT_DIR"
python3 "$WEBIDL_BINDER" "$JOLT_DIR/JoltJS.idl" "$OUT_DIR/glue"

echo "Generated $OUT_DIR/glue.cpp ($(wc -l < "$OUT_DIR/glue.cpp" | tr -d ' ') lines)"
