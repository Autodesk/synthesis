#!/usr/bin/env bash

set -u -o pipefail

RED=""
GREEN=""
YELLOW=""
NC=""

if [[ -t 1 && -z "${NO_COLOR:-}" ]]; then
    RED=$'\033[0;31m'
    GREEN=$'\033[0;32m'
    YELLOW=$'\033[0;33m'
    NC=$'\033[0m'
fi

SCRIPT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)/$(basename "${BASH_SOURCE[0]}")"
ROOT="$(cd "$(dirname "$SCRIPT_PATH")/.." >/dev/null 2>&1 && pwd)"
LOG_DIR="${CI_LOG_DIR:-$ROOT/.ci/logs}"
ORIGINAL_ARGS=("$@")
FAST=0
FAILED=0
JS_SETUP_OK=0
GLUEBALL_BIN=""
SERVER_PID=""

PASSED_CHECKS=()
FAILED_CHECKS=()
SKIPPED_CHECKS=()
CHECK_SUMMARY=()

usage() {
    cat <<EOF
Usage: $0 [--fast|--help]

  --fast    Run formatters and inexpensive sanity checks only.
  --help    Show this help text.
  (none)    Run the complete read-only CI suite.
EOF
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --fast)
            FAST=1
            shift
            ;;
        --help)
            usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1" >&2
            usage >&2
            exit 1
            ;;
    esac
done

if [[ "${SYNTHESIS_CI_SHELL:-}" != "1" ]]; then
    exec nix develop "$ROOT#ci" --command bash "$SCRIPT_PATH" "${ORIGINAL_ARGS[@]}"
fi

mkdir -p "$LOG_DIR"

format_status_line() {
    local name="$1"
    local dots_len=$((46 - ${#name}))
    if ((dots_len < 1)); then
        dots_len=1
    fi

    local dots
    dots="$(printf '%*s' "$dots_len" '' | tr ' ' '.')"
    printf "%s%s" "$name" "$dots"
}

run_check() {
    local name="$1"
    shift
    local log_file="$LOG_DIR/${name}.log"
    local started=$SECONDS
    local elapsed

    format_status_line "$name"

    if "$@" >"$log_file" 2>&1; then
        elapsed=$((SECONDS - started))
        PASSED_CHECKS+=("$name")
        CHECK_SUMMARY+=("passed|$name|${elapsed}s")
        printf "[%s✔%s] %ss\n" "$GREEN" "$NC" "$elapsed"
        return 0
    fi

    elapsed=$((SECONDS - started))
    FAILED=1
    FAILED_CHECKS+=("$name")
    CHECK_SUMMARY+=("failed|$name|${elapsed}s")
    printf "[%s✗%s] %ss\n" "$RED" "$NC" "$elapsed"
    sed 's/^/  /' "$log_file"
    return 1
}

skip_check() {
    local name="$1"
    local reason="$2"

    SKIPPED_CHECKS+=("$name")
    CHECK_SUMMARY+=("skipped|$name|-")
    format_status_line "$name"
    printf "[%s-%s] %s\n" "$YELLOW" "$NC" "$reason"
}

check_script_syntax() {
    bash -n "$SCRIPT_PATH"
}

check_merge_conflicts() {
    if git grep -n -E '^(<{7}|={7}|>{7})' -- \
        '*.bash' '*.cc' '*.cpp' '*.css' '*.h' '*.hpp' '*.js' '*.json' '*.py' '*.rs' '*.sh' '*.ts' '*.tsx' '*.toml' '*.yaml' '*.yml'; then
        return 1
    fi
}

check_branch_freshness() {
    if [[ "${GITHUB_EVENT_NAME:-}" != "pull_request" || "${GITHUB_BASE_REF:-}" != "dev" ]]; then
        echo "Not a pull request targeting dev; branch freshness check is not applicable."
        return 0
    fi

    local pull_request_number="${GITHUB_EVENT_NUMBER:-}"
    if [[ -z "$pull_request_number" ]]; then
        echo "GITHUB_EVENT_NUMBER is missing for a dev pull request." >&2
        return 1
    fi

    git fetch --no-tags origin \
        "refs/pull/${pull_request_number}/head:refs/remotes/origin/ci-pr-head" \
        "refs/heads/dev:refs/remotes/origin/dev"

    local behind
    behind="$(git rev-list --count refs/remotes/origin/ci-pr-head..refs/remotes/origin/dev)"
    echo "PR head is ${behind} commit(s) behind origin/dev."

    if ((behind > 100)); then
        echo "The PR is more than 100 commits behind dev. Merge or rebase origin/dev into the branch before continuing." >&2
        return 1
    fi
}

install_javascript_dependencies() {
    (cd "$ROOT/fission" && bun install --frozen-lockfile)
    (cd "$ROOT/exporter/SynthesisFusionAddin/web" && bun install --frozen-lockfile)
}

check_fission_format() {
    (cd "$ROOT/fission" && bun run fmt)
}

check_fission_lint() {
    (cd "$ROOT/fission" && bun run lint --diagnostic-level=error)
}

check_exporter_isort() {
    python3 "$ROOT/exporter/SynthesisFusionAddin/tools/verifyIsortFormatting.py"
}

format_exporter_isort() {
    (cd "$ROOT/exporter/SynthesisFusionAddin" && isort --settings-path pyproject.toml --skip-glob '*/node_modules/*' .)
}

check_exporter_black() {
    (cd "$ROOT/exporter/SynthesisFusionAddin" && python3 -m black --check --config pyproject.toml --extend-exclude '(^|/)node_modules(/|$)' .)
}

format_exporter_black() {
    (cd "$ROOT/exporter/SynthesisFusionAddin" && black --config pyproject.toml --extend-exclude '(^|/)node_modules(/|$)' .)
}

check_fusion_web_biome() {
    (cd "$ROOT/exporter/SynthesisFusionAddin/web" && bunx biome ci --error-on-warnings)
}

format_fusion_web_biome() {
    (cd "$ROOT/exporter/SynthesisFusionAddin/web" && bunx biome check --write)
}

check_exporter_mypy() {
    (cd "$ROOT/exporter/SynthesisFusionAddin" && mypy)
}

check_fusion_web_build() {
    (cd "$ROOT/exporter/SynthesisFusionAddin/web" && bun run build)
}

check_fission_build() {
    (cd "$ROOT/fission" && bun run build)
}

check_nix_flake() {
    (cd "$ROOT" && nix flake check)
}

prepare_assetpack() {
    if [[ -d "$ROOT/fission/public/Downloadables/mira" ]]; then
        echo "Using existing extracted assetpack."
        return 0
    fi

    git -C "$ROOT" lfs pull --include="fission/public/assetpack.zip"
    (cd "$ROOT/fission" && unzip -q -o public/assetpack.zip -d public/)

    [[ -d "$ROOT/fission/public/Downloadables/mira" ]]
}

check_fission_tests() {
    (cd "$ROOT/fission" && bun run test)
}

check_assetpack_tests() {
    (cd "$ROOT/fission" && VITE_RUN_ASSETPACK_TEST=true bun run test src/test/mirabuf/DefaultAssets.test.ts)
}

resolve_glueball_binary() {
    local output
    output="$(cd "$ROOT" && nix build .#glueball --no-link --print-out-paths)"
    GLUEBALL_BIN="$output/bin/glueball"
    [[ -x "$GLUEBALL_BIN" ]]
}

stop_glueball() {
    if [[ -z "$SERVER_PID" ]]; then
        return 0
    fi

    if kill -0 "$SERVER_PID" 2>/dev/null; then
        kill "$SERVER_PID" 2>/dev/null || true
        for _ in {1..10}; do
            if ! kill -0 "$SERVER_PID" 2>/dev/null; then
                SERVER_PID=""
                return 0
            fi
            sleep 1
        done
        kill -9 "$SERVER_PID" 2>/dev/null || true
    fi
    SERVER_PID=""
}

handle_signal() {
    local exit_code="$1"
    stop_glueball
    exit "$exit_code"
}

trap stop_glueball EXIT
trap 'handle_signal 130' INT
trap 'handle_signal 143' TERM

run_multiplayer_tests() {
    if [[ -z "$GLUEBALL_BIN" || ! -x "$GLUEBALL_BIN" ]]; then
        echo "Glueball binary is unavailable." >&2
        return 1
    fi

    local server_log="$LOG_DIR/glueball_server.log"
    "$GLUEBALL_BIN" --secure --headless --permanent-room TEST01 >"$server_log" 2>&1 &
    SERVER_PID=$!

    for _ in {1..30}; do
        if curl --insecure --fail --silent https://localhost:2610/ >/dev/null; then
            break
        fi
        if ! kill -0 "$SERVER_PID" 2>/dev/null; then
            cat "$server_log" >&2
            return 1
        fi
        sleep 1
    done

    if ! curl --insecure --fail --silent https://localhost:2610/ >/dev/null; then
        cat "$server_log" >&2
        return 1
    fi

    (cd "$ROOT/fission" && VITE_RUN_MULTIPLAYER_TEST=true bun run test src/test/multiplayer/)
}

write_summary() {
    local summary_file="${GITHUB_STEP_SUMMARY:-}"
    [[ -n "$summary_file" ]] || return 0

    {
        echo "## Synthesis CI"
        echo
        echo "| Status | Check | Duration |"
        echo "| --- | --- | ---: |"
        local summary_entry
        local status
        local check_name
        local duration
        for summary_entry in "${CHECK_SUMMARY[@]}"; do
            IFS='|' read -r status check_name duration <<<"$summary_entry"
            printf "| %s | \`%s\` | %s |\n" "$status" "$check_name" "$duration"
        done
    } >>"$summary_file"
}

if ((FAST == 1)); then
    run_check "ci_script_syntax" check_script_syntax
    run_check "merge_conflicts" check_merge_conflicts
    run_check "exporter_isort_format" format_exporter_isort
    run_check "exporter_black_format" format_exporter_black
    run_check "fission_biome_format" bash -c "cd '$ROOT/fission' && bun run fmt:fix"
    run_check "fusion_web_biome_format" format_fusion_web_biome
else
    if ! run_check "branch_freshness" check_branch_freshness; then
        write_summary
        exit 1
    fi

    run_check "ci_script_syntax" check_script_syntax
    run_check "merge_conflicts" check_merge_conflicts

    if run_check "javascript_dependencies" install_javascript_dependencies; then
        JS_SETUP_OK=1
    fi

    if ((JS_SETUP_OK == 1)); then
        run_check "fission_biome_format" check_fission_format
        run_check "fission_biome_lint" check_fission_lint
        run_check "fusion_web_biome" check_fusion_web_biome
        run_check "fusion_web_build" check_fusion_web_build
        run_check "fission_build" check_fission_build
    else
        skip_check "fission_biome_format" "JavaScript dependency setup failed"
        skip_check "fission_biome_lint" "JavaScript dependency setup failed"
        skip_check "fusion_web_biome" "JavaScript dependency setup failed"
        skip_check "fusion_web_build" "JavaScript dependency setup failed"
        skip_check "fission_build" "JavaScript dependency setup failed"
    fi

    run_check "exporter_isort" check_exporter_isort
    run_check "exporter_black" check_exporter_black
    run_check "exporter_mypy" check_exporter_mypy
    run_check "nix_flake_check" check_nix_flake

    if run_check "assetpack_prepare" prepare_assetpack; then
        if ((JS_SETUP_OK == 1)); then
            run_check "fission_tests" check_fission_tests
            run_check "assetpack_tests" check_assetpack_tests
        else
            skip_check "fission_tests" "JavaScript dependency setup failed"
            skip_check "assetpack_tests" "JavaScript dependency setup failed"
        fi
    else
        skip_check "fission_tests" "Assetpack preparation failed"
        skip_check "assetpack_tests" "Assetpack preparation failed"
    fi

    if run_check "glueball_binary" resolve_glueball_binary; then
        if ((JS_SETUP_OK == 1)); then
            run_check "multiplayer_tests" run_multiplayer_tests
        else
            skip_check "multiplayer_tests" "JavaScript dependency setup failed"
        fi
    else
        skip_check "multiplayer_tests" "Glueball build failed"
    fi
fi

write_summary

if ((FAILED == 1)); then
    echo
    printf "%sCI failed%s\n" "$RED" "$NC"
    exit 1
fi

echo
printf "%sCI passed%s\n" "$GREEN" "$NC"
