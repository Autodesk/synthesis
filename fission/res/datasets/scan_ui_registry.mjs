#!/usr/bin/env node
/*
 Scans the fission UI codebase to extract panel titles and HUD menu paths.
 Writes to fission/res/ui_registry_fission.json, merging existing selectors/macros.
*/
import fs from "fs"
import path from "path"

const ROOT = path.resolve(process.cwd())
const SRC = path.join(ROOT, "fission/src")
const REGISTRY_PATH = path.join(ROOT, "fission/res/ui_registry_fission.json")

function read(file) { return fs.readFileSync(file, "utf8") }
function exists(p) { return fs.existsSync(p) }

function walk(dir, acc = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name)
    if (entry.isDirectory()) walk(full, acc)
    else if (entry.name.endsWith(".tsx") || entry.name.endsWith(".ts")) acc.push(full)
  }
  return acc
}

function slugify(text) {
  return text.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/(^-|-$)/g, "")
}

function scanPanels() {
  const files = walk(path.join(SRC, "ui/panels"))
  const panels = []
  const titleRe = /configureScreen\(panel!?\s*,\s*\{[^}]*title:\s*"([^"]+)"/g
  for (const f of files) {
    const text = read(f)
    let m
    while ((m = titleRe.exec(text)) !== null) {
      const label = m[1]
      panels.push({ id: slugify(label), label })
    }
  }
  // Deduplicate by label
  const seen = new Set()
  return panels.filter(p => { const k = p.label; if (seen.has(k)) return false; seen.add(k); return true })
}

function scanHUDMenu() {
  const hudFile = path.join(SRC, "ui/components/MainHUD.tsx")
  if (!exists(hudFile)) return []
  const text = read(hudFile)
  // Find MainHUDButton value labels
  const valRe = /<MainHUDButton\s+[\s\S]*?value=\{\"([^\"]+)\"\}/g
  const arr = []
  let m
  while ((m = valRe.exec(text)) !== null) arr.push(m[1])
  return arr.map(v => ({ label: v, path: ["HUD", v] }))
}

function main() {
  const existing = exists(REGISTRY_PATH) ? JSON.parse(read(REGISTRY_PATH)) : { panels: [], menu_paths: [], selectors: [], hotkeys: [], macros: [] }
  const panels = scanPanels()
  const menu_paths = scanHUDMenu()
  const next = {
    panels,
    menu_paths,
    selectors: existing.selectors || [],
    hotkeys: existing.hotkeys || [],
    macros: existing.macros || [],
  }
  fs.writeFileSync(REGISTRY_PATH, JSON.stringify(next, null, 2) + "\n", "utf8")
  console.log(JSON.stringify({ ok: true, panels: panels.length, menu_paths: menu_paths.length }))
}

main()


