#!/usr/bin/env node
/*
 Generates JSONL conversations for the Synthesis fission education copilot.
 - Focus: navigation and part explanations
 - Uses ui registry at fission/res/ui_registry_fission.json
 - Outputs multiple shards with configurable size
*/

import fs from "fs"
import path from "path"

const ROOT = path.resolve(process.cwd())
const REGISTRY_PATH = path.join(ROOT, "fission/res/ui_registry_fission.json")
const DOC_KEYS_PATH = path.join(ROOT, "fission/res/doc_keys.json")

function readJson(p) {
  return JSON.parse(fs.readFileSync(p, "utf8"))
}

function ensureDir(p) {
  if (!fs.existsSync(p)) fs.mkdirSync(p, { recursive: true })
}

function pick(arr) {
  return arr[Math.floor(Math.random() * arr.length)]
}

function slugify(text) {
  return text.toLowerCase().replace(/[^a-z0-9]+/g, "_").replace(/^_+|_+$/g, "")
}

function id(prefix, label, n) {
  const slug = slugify(label)
  return `${prefix}.${slug}.${String(n).padStart(3, "0")}`
}

function sysMsg() {
  return { from: "system", value: "You are the Synthesis Tutorial Copilot. Your focus is navigation and part explanations. Ask before running actions." }
}

function navOpen(panel, n) {
  return {
    id: id("nav.open", panel.label, n),
    tags: ["nav", "synthesis", "ui"],
    meta: { requires_tool: true, difficulty: "easy" },
    conversations: [
      sysMsg(),
      { from: "user", value: `Open ${panel.label}.` },
      { from: "assistant", value: `I can open ${panel.label}. Proceed? (yes/no)` },
      { from: "user", value: "yes" },
      { from: "assistant", value: `Opening ${panel.label}.`, tool_call: { name: "open_panel", arguments: { panelId: panel.id } } },
      { from: "tool", value: "{\"ok\":true}" },
      { from: "assistant", value: `Path: ${menuPathText(panel.label)}.` },
    ],
  }
}

function menuPathText(label) {
  const found = registry.menu_paths.find(m => m.label === label)
  return found ? found.path.join(" → ") : `HUD → ${label}`
}

function highlight(selector, n) {
  const item = registry.selectors.find(s => s.id === selector)
  if (!item) throw new Error(`Selector not in registry: ${selector}`)
  const panelLabel = panelsById.get(item.panelId)?.label ?? ""
  return {
    id: id("nav.highlight", item.label, n),
    tags: ["nav", "synthesis", "ui"],
    meta: { requires_tool: true, difficulty: "easy" },
    conversations: [
      sysMsg(),
      { from: "user", value: `Where is ${item.label}?` },
      { from: "assistant", value: `I can highlight ${item.label}. Proceed? (yes/no)` },
      { from: "user", value: "yes" },
      { from: "assistant", value: `Highlighting ${item.label}.`, tool_call: { name: "highlight", arguments: { selector: item.id, durationMs: 800 } } },
      { from: "tool", value: "{\"ok\":true}" },
      { from: "assistant", value: `Path: ${menuPathText(panelLabel)}. Use ${item.label} to continue.` },
    ],
  }
}

function explainPanel(panel, n, withDocs = false) {
  const base = [
    sysMsg(),
    { from: "user", value: `What does ${panel.label} do?` },
    { from: "assistant", value: `${panel.label} manages related settings or actions. Typical use: ${panelUse(panel)}. Pitfall: ${panelPitfall(panel)}.${withDocs ? " Want an overview link?" : ""}` },
  ]
  if (!withDocs) {
    return { id: id("explain.panel", panel.label, n), tags: ["explain", "synthesis", "ui"], meta: { requires_tool: false, difficulty: "easy" }, conversations: base }
  }
  return {
    id: id("explain.panel", panel.label, n),
    tags: ["explain", "synthesis", "ui"],
    meta: { requires_tool: true, difficulty: "easy" },
    conversations: [
      ...base,
      { from: "user", value: "yes" },
      { from: "assistant", value: "Opening docs.", tool_call: { name: "open_docs", arguments: { url: pick(Object.keys(docKeys)) } } },
      { from: "tool", value: "{\"ok\":true}" },
    ],
  }
}

function panelUse(panel) {
  const l = panel.label
  if (l === "Spawn Asset") return "spawn robots or fields from cache, defaults, or file"
  if (l === "Configure Assets") return "edit robot or field settings by mode"
  if (l === "Match Mode Config") return "choose rules and start a match"
  if (l === "Wiring Panel") return "map devices for simulation I/O"
  if (l === "Driver Station") return "view connection state and control indicators"
  if (l === "Auto Testing") return "run automated driver input tests"
  if (l === "Developer Tool") return "run utilities for debugging"
  if (l === "Debug Tools") return "toggle debug features and test UI"
  return "perform its titled function within the simulator"
}

function panelPitfall(panel) {
  const l = panel.label
  if (l === "Spawn Asset") return "spawning a field removes other fields"
  if (l === "Configure Assets") return "unsaved changes are lost if you cancel"
  if (l === "Match Mode Config") return "cannot modify rules while a match is running"
  if (l === "Wiring Panel") return "unmapped devices won’t respond in sim"
  if (l === "Driver Station") return "requires a running brain to show inputs"
  return "settings may not apply if the target is not selected"
}

function disambiguate(n) {
  return {
    id: `disambiguate.robots_spawn_vs_configure.${String(n).padStart(3, "0")}`,
    tags: ["disambiguate", "synthesis", "ui"],
    meta: { requires_tool: false, difficulty: "easy" },
    conversations: [
      sysMsg(),
      { from: "user", value: "Open robots." },
      { from: "assistant", value: "Do you want to spawn a robot (HUD → Spawn Asset → Robots) or configure a robot (HUD → Configure Assets → ROBOTS)?" },
    ],
  }
}

function twoStepOpenAndHighlight(panel, selector, n) {
  const sel = registry.selectors.find(s => s.id === selector)
  return {
    id: id("nav.open_and_highlight", sel?.label ?? "control", n),
    tags: ["nav", "synthesis", "ui"],
    meta: { requires_tool: true, difficulty: "easy" },
    conversations: [
      sysMsg(),
      { from: "user", value: `Help me find ${sel?.label ?? "that control"}.` },
      { from: "assistant", value: `I can open ${panelsById.get(panel).label} and highlight ${sel?.label}. Proceed? (yes/no)` },
      { from: "user", value: "yes" },
      { from: "assistant", value: `Opening ${panelsById.get(panel).label}.`, tool_call: { name: "open_panel", arguments: { panelId: panel } } },
      { from: "tool", value: "{\"ok\":true}" },
      { from: "assistant", value: `Highlighting ${sel?.label}.`, tool_call: { name: "highlight", arguments: { selector: selector, durationMs: 800 } } },
      { from: "tool", value: "{\"ok\":true}" },
      { from: "assistant", value: `Path: ${menuPathText(panelsById.get(panel).label)}. Click ${sel?.label} to continue.` },
    ],
  }
}

function buildAllExamples(targetSize) {
  const items = []
  let n = 1

  // Seed: at least one per panel (nav open)
  for (const p of registry.panels) items.push(navOpen(p, n++))

  // Seed: selectors
  for (const s of registry.selectors) items.push(highlight(s.id, n++))

  // Seed: explains
  for (const p of registry.panels) items.push(explainPanel(p, n++, false))
  for (const p of registry.panels.slice(0, 4)) items.push(explainPanel(p, n++, true))

  // Seed: disambiguation
  items.push(disambiguate(n++))

  // Two-step examples
  items.push(twoStepOpenAndHighlight("spawn-asset", "toggle-robots", n++))
  items.push(twoStepOpenAndHighlight("spawn-asset", "import-from-file", n++))

  // Expand to targetSize with variations
  const navPrompts = [
    (p) => ({ user: `How do I get to ${p.label}?`, ask: `I can open ${p.label}. Proceed? (yes/no)`, follow: `Path: ${menuPathText(p.label)}.` }),
    (p) => ({ user: `Show me ${p.label}.`, ask: `I can open ${p.label}. Proceed? (yes/no)`, follow: `Opened ${p.label}.` }),
    (p) => ({ user: `Where is ${p.label}?`, ask: `I can open ${p.label}. Proceed? (yes/no)`, follow: `Open via ${menuPathText(p.label)}.` }),
  ]

  while (items.length < targetSize) {
    const type = Math.random()
    if (type < 0.6) {
      // one tool call (open or highlight)
      if (Math.random() < 0.7) {
        const p = pick(registry.panels)
        const t = pick(navPrompts)(p)
        items.push({
          id: id("nav.open", p.label, n++),
          tags: ["nav", "synthesis", "ui"],
          meta: { requires_tool: true, difficulty: "easy" },
          conversations: [
            sysMsg(),
            { from: "user", value: t.user },
            { from: "assistant", value: t.ask },
            { from: "user", value: "yes" },
            { from: "assistant", value: `Opening ${p.label}.`, tool_call: { name: "open_panel", arguments: { panelId: p.id } } },
            { from: "tool", value: "{\"ok\":true}" },
            { from: "assistant", value: t.follow },
          ],
        })
      } else {
        const s = pick(registry.selectors)
        items.push(highlight(s.id, n++))
      }
    } else if (type < 0.7) {
      // two tool calls (open + highlight)
      const s = pick(registry.selectors)
      items.push(twoStepOpenAndHighlight(s.panelId, s.id, n++))
    } else if (type < 0.9) {
      // explain without tools
      items.push(explainPanel(pick(registry.panels), n++, false))
    } else if (type < 0.95) {
      // explain with docs
      items.push(explainPanel(pick(registry.panels), n++, true))
    } else {
      // disambiguation
      items.push(disambiguate(n++))
    }
  }
  return items
}

function writeSharded(items, outDir, shardSize) {
  ensureDir(outDir)
  let idx = 0
  let shard = 1
  while (idx < items.length) {
    const slice = items.slice(idx, idx + shardSize)
    const outPath = path.join(outDir, `edu_chat_nav_dataset.large.part${String(shard).padStart(2, "0")}.jsonl`)
    const text = slice.map(o => JSON.stringify(o)).join("\n") + "\n"
    fs.writeFileSync(outPath, text, "utf8")
    idx += shardSize
    shard += 1
  }
}

// Main
const args = process.argv.slice(2)
const outDir = path.resolve(args[args.indexOf("--outDir") + 1] || path.join(ROOT, "fission/res/datasets"))
const sizeArg = Number(args[args.indexOf("--size") + 1] || 3000)
const shardArg = Number(args[args.indexOf("--shard") + 1] || 500)

const registry = readJson(REGISTRY_PATH)
const docKeys = readJson(DOC_KEYS_PATH)
const panelsById = new Map(registry.panels.map(p => [p.id, p]))

const items = buildAllExamples(sizeArg)
writeSharded(items, outDir, shardArg)

console.log(JSON.stringify({ ok: true, written: items.length, outDir }))

