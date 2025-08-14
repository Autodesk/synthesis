import { Box, List, ListItemButton, ListItemText, Paper, Stack, TextField } from "@mui/material"
import Fuse from "fuse.js"
import type React from "react"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import MatchMode from "@/systems/match_mode/MatchMode"
import World from "@/systems/World"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import SettingsModal from "@/ui/modals/configuring/SettingsModal"
import type { ConfigurationType } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import DebugPanel from "@/ui/panels/DebugPanel"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import MatchModeConfigPanel from "../panels/configuring/MatchModeConfigPanel"

type CommandDefinition = {
    id: string
    label: string
    description?: string
    keywords?: string[]
    perform: () => void
}

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
    }, [])

    const openPalette = useCallback(() => {
        setIsOpen(true)
        setTimeout(() => inputRef.current?.focus(), 0)
    }, [])

    const openImportPanel = useCallback(
        (configurationType: ConfigurationType) => {
            openPanel<void, { configurationType: ConfigurationType }>(ImportMirabufPanel, { configurationType })
        },
        [openPanel]
    )

    const commands = useMemo<CommandDefinition[]>(() => {
        const list: CommandDefinition[] = [
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

        // Dynamic per-assembly configuration commands (robots and field)
        if (isOpen && World.isAlive && World.sceneRenderer) {
            const robots = World.sceneRenderer.mirabufSceneObjects.getRobots() || []
            for (const r of robots) {
                const name = r.assemblyName || "Robot"
                const nameTokens = String(name)
                    .split(/\s+|[-_]/g)
                    .filter(Boolean)
                list.push({
                    id: `configure-robot-${r.id}`,
                    label: `Configure ${name}`,
                    description: `Open configuration for robot ${name}.`,
                    keywords: ["configure", "robot", ...nameTokens.map(t => t.toLowerCase())],
                    perform: () =>
                        openPanel(ConfigurePanel, {
                            configurationType: "ROBOTS",
                            selectedAssembly: r,
                        }),
                })
                list.push({
                    id: `remove-robot-${r.id}`,
                    label: `Remove ${name}`,
                    description: `Remove the robot ${name}.`,
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
        }

        return list
    }, [addToast, openPanel, openModal, openImportPanel, isOpen])

    const fuse = useMemo(() => {
        return new Fuse(commands, {
            keys: ["label", "description", "keywords"],
            threshold: 0.3,
            ignoreLocation: true,
            includeMatches: true,
            shouldSort: false,
            includeScore: true,
        })
    }, [commands])

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase()
        if (!q) return commands
        return fuse
            .search(q)
            .reverse()
            .map(r => r.item)
    }, [commands, fuse, query])

    const visible = useMemo(() => filtered.slice(0, 5), [filtered])

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
