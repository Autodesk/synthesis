import { execFileSync } from "node:child_process"
import { chmodSync, copyFileSync, existsSync, mkdirSync } from "node:fs"
import { join } from "node:path"

const HOOKS = ["pre-commit"]

function gitCommonDir(): string | null {
    try {
        return execFileSync("git", ["rev-parse", "--path-format=absolute", "--git-common-dir"], {
            encoding: "utf8",
            stdio: ["ignore", "pipe", "ignore"],
        }).trim()
    } catch {
        return null
    }
}

const commonDir = gitCommonDir()
if (!commonDir) {
    console.log("hooks:install: not a git repository, skipping.")
    process.exit(0)
}

const hooksDir = join(commonDir, "hooks")
mkdirSync(hooksDir, { recursive: true })

for (const hook of HOOKS) {
    const src = join(import.meta.dirname, hook)
    if (!existsSync(src)) {
        console.warn(`hooks:install: source hook missing: ${src}`)
        continue
    }
    const dest = join(hooksDir, hook)
    copyFileSync(src, dest)
    chmodSync(dest, 0o755)
    console.log(`hooks:install: installed ${hook} -> ${dest}`)
}
