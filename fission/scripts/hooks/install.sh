#!/usr/bin/env sh
#
# Installs the fission Biome pre-commit hook.

set -e

# git runs hooks from the repo root so the path is relative to the root
HOOK_COMMAND='sh ./fission/scripts/hooks/pre-commit'

if ! git rev-parse --git-dir >/dev/null 2>&1; then
  exit 0
fi

hooks_dir=$(git config --get core.hooksPath 2>/dev/null || true)
if [ -n "$hooks_dir" ]; then
  case "$hooks_dir" in
    /*) ;;
    *) hooks_dir="$(git rev-parse --show-toplevel)/$hooks_dir" ;;
  esac
  hook_file="$hooks_dir/pre-commit"
else
  # worktree-safe
  hook_file=$(git rev-parse --git-path hooks/pre-commit 2>/dev/null) || exit 0
fi

mkdir -p "$(dirname "$hook_file")"

if [ ! -e "$hook_file" ]; then
  printf '#!/bin/sh\n' >"$hook_file"
fi
chmod +x "$hook_file"

# hook already installed
if grep -q 'fission/scripts/hooks/pre-commit' "$hook_file"; then
  exit 0
fi

printf '%s\n' "$HOOK_COMMAND" >>"$hook_file"

echo "prepare: installed fission Biome pre-commit hook in $hook_file"
