import { Box, List, ListItemButton, ListItemText, Paper, Stack, TextField } from "@mui/material"
import Fuse from "fuse.js"
import type React from "react"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import MatchMode from "@/systems/match_mode/MatchMode"
import World from "@/systems/World"
import InputSystem from "@/systems/input/InputSystem"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import SettingsModal from "@/ui/modals/configuring/SettingsModal"
import type { ConfigurationType } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import DebugPanel from "@/ui/panels/DebugPanel"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import MatchModeConfigPanel from "../panels/configuring/MatchModeConfigPanel"
import CommandRegistry, { type CommandDefinition, type CommandProvider } from "@/ui/components/CommandRegistry"

function isTextInputTarget(target: EventTarget | null): boolean {
    if (!(target instanceof HTMLElement)) return false
    const tagName = target.tagName.toLowerCase()
    const editable = target.getAttribute("contenteditable")
    return tagName === "input" || tagName === "textarea" || editable === "" || editable === "true"
}

const CommandPalette: React.FC = () => {
    const { addToast, openPanel, openModal, modal } = useUIContext()
    const { isMainMenuOpen } = useStateContext()

    const [isOpen, setIsOpen] = useState<boolean>(false)
    const [query, setQuery] = useState<string>("")
    const [activeIndex, setActiveIndex] = useState<number>(0)
    const inputRef = useRef<HTMLInputElement | null>(null)
    const containerRef = useRef<HTMLDivElement | null>(null)

    const closePalette = useCallback(() => {
        setIsOpen(false)
        setQuery("")
        setActiveIndex(0)
        InputSystem.setCommandPaletteOpen(false)
    }, [])

    const openPalette = useCallback(() => {
        setIsOpen(true)
        InputSystem.setCommandPaletteOpen(true)
        setTimeout(() => inputRef.current?.focus(), 0)
    }, [])

    const openImportPanel = useCallback(
        (configurationType: ConfigurationType) => {
            openPanel(ImportMirabufPanel, { configurationType })
        },
        [openPanel]
    )

    // Register initial static commands with the registry
    useEffect(() => {
        const staticCommands: CommandDefinition[] = [
            {
                id: "open-debug-panel",
                label: "Open Debug Panel",
                description: "Open the Debug tools panel.",
                keywords: ["panel", "debug"],
                perform: () => openPanel(DebugPanel, undefined),
            },
            {
                id: "toggle-drag-mode",
                label: "Toggle Drag Mode",
                description: "Enable or disable drag mode.",
                keywords: ["drag", "mode", "toggle", "move"],
                perform: () => {
                    const dragSystem = World.dragModeSystem
                    if (!dragSystem) return
                    dragSystem.enabled = !dragSystem.enabled
                    const status = dragSystem.enabled ? "enabled" : "disabled"
                    addToast("info", "Drag Mode", `Drag mode has been ${status}`)
                },
            },
            {
                id: "spawn-asset-robots",
                label: "Spawn Asset (Robots)",
                description: "Open asset spawn panel scoped to robots.",
                keywords: ["spawn", "asset", "robot", "import", "mirabuf"],
                perform: () => openImportPanel("ROBOTS"),
            },
            {
                id: "spawn-asset-fields",
                label: "Spawn Asset (Fields)",
                description: "Open asset spawn panel scoped to fields.",
                keywords: ["spawn", "asset", "field", "import", "mirabuf"],
                perform: () => openImportPanel("FIELDS"),
            },
            {
                id: "configure-assets",
                label: "Configure Assets",
                description: "Open the asset configuration panel.",
                keywords: ["configure", "asset", "config"],
                perform: () => openPanel(ConfigurePanel, {}),
            },
            {
                id: "configure-robots",
                label: "Configure Robots",
                description: "Open the configuration panel scoped to spawned robots.",
                keywords: ["configure", "robot", "robots", "config"],
                perform: () => {
                    const robots = World.sceneRenderer.mirabufSceneObjects.getRobots()
                    if (!robots || robots.length === 0) {
                        addToast("warning", "No Robots", "No robots are currently spawned.")
                        return
                    }
                    openPanel(ConfigurePanel, {
                        configurationType: "ROBOTS",
                        selectedAssembly: robots.length === 1 ? robots[0] : undefined,
                    })
                },
            },
            {
                id: "configure-field",
                label: "Configure Field",
                description: "Open the configuration panel scoped to the spawned field.",
                keywords: ["configure", "field", "config"],
                perform: () => {
                    const field = World.sceneRenderer.mirabufSceneObjects.getField()
                    if (!field) {
                        addToast("warning", "No Field", "No field is currently spawned.")
                        return
                    }
                    openPanel(ConfigurePanel, {
                        configurationType: "FIELDS",
                        selectedAssembly: field,
                    })
                },
            },
            {
                id: "open-settings",
                label: "Open Settings",
                description: "Open the Settings modal.",
                keywords: ["settings", "preferences", "config"],
                perform: () => openModal(SettingsModal, undefined),
            },
            {
                id: "toggle-match-mode",
                label: "Toggle Match Mode",
                description: "Toggle match mode, allowing you to simulate and run a full match.",
                keywords: ["match", "mode", "start", "play", "game", "simulate", "toggle"],
                perform: () => {
                    if (MatchMode.getInstance().isMatchEnabled()) {
                        MatchMode.getInstance().sandboxModeStart()
                        addToast("info", "Match Mode Cancelled")
                    } else {
                        openPanel(MatchModeConfigPanel, undefined)
                    }
                },
            },
        ]

        const registry = CommandRegistry.get()
        const dispose = registry.registerCommands(staticCommands)
        return () => dispose()
    }, [addToast, openPanel, openModal, openImportPanel])

    // Register dynamic per-assembly commands via a provider
    useEffect(() => {
        const provider: CommandProvider = () => {
            if (!World.isAlive || !World.sceneRenderer) return []
            const list: CommandDefinition[] = []

            const robots = World.sceneRenderer.mirabufSceneObjects.getRobots() || []
            for (const r of robots) {
                const name = r.assemblyName || "Robot"
                const nameTokens = String(name)
                    .split(/\s+|[-_]/g)
                    .filter(Boolean)
                list.push({
                    id: `configure-robot-${r.id}`,
                    label: `Configure ${r.nameTag?.text()} (${name})`,
                    description: `Open configuration for robot ${r.nameTag?.text()} (${name}).`,
                    keywords: ["configure", "robot", ...nameTokens.map(t => t.toLowerCase())],
                    perform: () =>
                        openPanel(ConfigurePanel, {
                            configurationType: "ROBOTS",
                            selectedAssembly: r,
                        }),
                })
                list.push({
                    id: `remove-robot-${r.id}`,
                    label: `Remove ${r.nameTag?.text()} (${name})`,
                    description: `Remove the robot ${r.nameTag?.text()} (${name}).`,
                    keywords: ["remove", "delete", "robot", ...nameTokens.map(t => t.toLowerCase())],
                    perform: () => {
                        World.sceneRenderer.removeSceneObject(r.id)
                    },
                })
            }

            const field = World.sceneRenderer.mirabufSceneObjects.getField()
            if (field) {
                const name = field.assemblyName || "Field"
                const nameTokens = String(name)
                    .split(/\s+|[-_]/g)
                    .filter(Boolean)
                list.push({
                    id: `configure-field-${field.id}`,
                    label: `Configure ${name}`,
                    description: `Open configuration for field ${name}.`,
                    keywords: ["configure", "field", ...nameTokens.map(t => t.toLowerCase())],
                    perform: () =>
                        openPanel(ConfigurePanel, {
                            configurationType: "FIELDS",
                            selectedAssembly: field,
                        }),
                })
                list.push({
                    id: `remove-field-${field.id}`,
                    label: `Remove ${name}`,
                    description: `Remove the field ${name}.`,
                    keywords: ["remove", "delete", "field", ...nameTokens.map(t => t.toLowerCase())],
                    perform: () => {
                        World.sceneRenderer.removeSceneObject(field.id)
                    },
                })
            }

            return list
        }

        const registry = CommandRegistry.get()
        const dispose = registry.registerProvider(provider)
        return () => dispose()
    }, [openPanel])

    // Subscribe to registry updates to refresh palette command list
    const [registryTick, setRegistryTick] = useState(0)
    useEffect(() => {
        const registry = CommandRegistry.get()
        return registry.subscribe(() => setRegistryTick(t => t + 1))
    }, [])

    const commands = useMemo<CommandDefinition[]>(() => {
        return CommandRegistry.get().getCommands()
    }, [registryTick])

    const fuse = useMemo(() => {
        return new Fuse(commands, {
            keys: ["label", "description", "keywords"],
            threshold: 0.3,
            ignoreLocation: true,
            includeMatches: true,
            shouldSort: true,
            includeScore: true,
        })
    }, [commands])

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase()
        if (!q) return commands
        return fuse.search(q).map(r => r.item)
    }, [commands, fuse, query])

    const visible = useMemo(() => {
        return filtered.slice(0, 5).reverse()
    }, [filtered])

    const execute = useCallback(
        (index: number) => {
            const cmd = visible[index]
            if (cmd) {
                cmd.perform()
            } else {
                addToast("error", "Command Not Found", "The command you entered was not found.")
            }

            closePalette()
        },
        [visible, closePalette, addToast]
    )

    useEffect(() => {
        const onKeyDown = (e: KeyboardEvent) => {
            if (e.key === "/") {
                if (isTextInputTarget(e.target)) return
                if (!World.isAlive) return
                if (isMainMenuOpen) return
                if (modal) return
                e.preventDefault()
                openPalette()
            } else if (e.key === "Escape") {
                if (isOpen) {
                    e.preventDefault()
                    closePalette()
                }
            }
        }
        window.addEventListener("keydown", onKeyDown)
        return () => window.removeEventListener("keydown", onKeyDown)
    }, [isOpen, isMainMenuOpen, modal, openPalette, closePalette])

    useEffect(() => {
        if ((isMainMenuOpen || modal) && isOpen) {
            closePalette()
        }
    }, [isMainMenuOpen, modal, isOpen, closePalette])

    useEffect(() => {
        if (!isOpen) return
        setActiveIndex(visible.length > 0 ? visible.length - 1 : 0)
    }, [isOpen, visible.length])

    useEffect(() => {
        if (!isOpen) return
        const onPointerDown = (e: PointerEvent) => {
            const target = e.target as Node | null
            if (containerRef.current && target && !containerRef.current.contains(target)) {
                closePalette()
            }
        }
        document.addEventListener("pointerdown", onPointerDown)
        return () => document.removeEventListener("pointerdown", onPointerDown)
    }, [isOpen, closePalette])

    const onInputKeyDown = useCallback(
        (e: React.KeyboardEvent<HTMLInputElement>) => {
            if (e.key === "ArrowDown") {
                e.preventDefault()
                setActiveIndex(i => {
                    const count = visible.length
                    if (count <= 0) return 0
                    return (i + 1 + count) % count
                })
            } else if (e.key === "ArrowUp") {
                e.preventDefault()
                setActiveIndex(i => {
                    const count = visible.length
                    if (count <= 0) return 0
                    return (i - 1 + count) % count
                })
            } else if (e.key === "Enter") {
                e.preventDefault()
                execute(activeIndex)
            } else if (e.key === "Escape") {
                e.preventDefault()
                closePalette()
            }
        },
        [activeIndex, execute, visible.length, closePalette]
    )

    if (!isOpen) return null

    return (
        <Box
            component="div"
            sx={{
                position: "fixed",
                left: 0,
                right: 0,
                bottom: 0,
                pointerEvents: "none",
                zIndex: 1400,
            }}
        >
            <Stack direction="column" alignItems="center" sx={{ mb: 2, pointerEvents: "auto" }}>
                <Paper elevation={8} sx={{ width: "min(800px, 95vw)" }} ref={containerRef}>
                    {visible.length > 0 && (
                        <List dense disablePadding>
                            {visible.map((c, i) => (
                                <ListItemButton
                                    key={c.id}
                                    selected={i === activeIndex}
                                    onMouseEnter={() => setActiveIndex(i)}
                                    onClick={() => execute(i)}
                                >
                                    <ListItemText primary={c.label} secondary={c.description} />
                                </ListItemButton>
                            ))}
                        </List>
                    )}
                    <TextField
                        inputRef={inputRef}
                        fullWidth
                        placeholder="Type a command… (Esc to close)"
                        variant="outlined"
                        value={query}
                        onChange={e => {
                            setQuery(e.target.value)
                        }}
                        onKeyDown={onInputKeyDown}
                    />
                </Paper>
            </Stack>
        </Box>
    )
}

export default CommandPalette
