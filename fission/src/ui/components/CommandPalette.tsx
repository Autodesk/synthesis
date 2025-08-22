import { Box, List, ListItemButton, ListItemText, Paper, Stack, TextField } from "@mui/material"
import Fuse from "fuse.js"
import type React from "react"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import World from "@/systems/World"
import InputSystem from "@/systems/input/InputSystem"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import CommandRegistry, { type CommandDefinition } from "@/ui/components/CommandRegistry"
import "@/ui/panels/DebugPanel"
import "@/ui/modals/configuring/SettingsModal"
import "@/ui/panels/mirabuf/ImportMirabufPanel"
import "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import "@/ui/panels/configuring/MatchModeConfigPanel"

function isTextInputTarget(target: EventTarget | null): boolean {
    if (!(target instanceof HTMLElement)) return false
    const tagName = target.tagName.toLowerCase()
    const editable = target.getAttribute("contenteditable")
    return tagName === "input" || tagName === "textarea" || editable === "" || editable === "true"
}

const CommandPalette: React.FC = () => {
    const { addToast, modal } = useUIContext()
    const { isMainMenuOpen } = useStateContext()

    const [isOpen, setIsOpen] = useState<boolean>(false)
    const [query, setQuery] = useState<string>("")
    const [activeIndex, setActiveIndex] = useState<number>(0)
    const inputRef = useRef<HTMLInputElement | null>(null)
    const containerRef = useRef<HTMLDivElement | null>(null)
    const listItemRefs = useRef<(HTMLDivElement | null)[]>([])

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

    // Register command(s) not owned elsewhere
    useEffect(() => {
        const staticCommands: CommandDefinition[] = [
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
        ]

        const registry = CommandRegistry.get()
        const dispose = registry.registerCommands(staticCommands)
        return () => dispose()
    }, [addToast])

    // Subscribe to registry updates to refresh palette command list
    const [registryTick, setRegistryTick] = useState(0)
    useEffect(() => {
        const registry = CommandRegistry.get()
        return registry.subscribe(() => setRegistryTick(t => t + 1))
    }, [])

    // Force a refresh when opening, so dynamic providers reflect current assemblies
    useEffect(() => {
        if (isOpen) {
            setRegistryTick(t => t + 1)
        }
    }, [isOpen])

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
        return [...filtered].reverse()
    }, [filtered])

    const execute = useCallback(
        (index: number) => {
            const cmd = visible[index]
            World.analyticsSystem?.event("Command Executed", { command: cmd?.label ?? "Unknown" })
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
        listItemRefs.current = listItemRefs.current.slice(0, visible.length)
    }, [visible.length])

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

    useEffect(() => {
        if (!isOpen) return
        const activeItem = listItemRefs.current[activeIndex]
        if (activeItem) {
            activeItem.scrollIntoView({ block: "nearest" })
        }
    }, [isOpen, activeIndex])

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
                        <List
                            dense
                            disablePadding
                            sx={{
                                maxHeight: "300px",
                                overflowY: "auto",
                            }}
                        >
                            {visible.map((c, i) => (
                                <ListItemButton
                                    key={c.id}
                                    selected={i === activeIndex}
                                    onMouseEnter={() => setActiveIndex(i)}
                                    onClick={() => execute(i)}
                                    ref={_element => {
                                        listItemRefs.current[i] = _element
                                    }}
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
                        autoComplete="off"
                        onKeyDown={onInputKeyDown}
                        sx={{
                            "& .MuiOutlinedInput-root": {
                                borderTopLeftRadius: 0,
                                borderTopRightRadius: 0,
                                "&:hover fieldset, &.Mui-focused fieldset": {
                                    borderColor: "rgba(0, 0, 0, 0.9)",
                                    borderWidth: "1px",
                                },
                            },
                        }}
                    />
                </Paper>
            </Stack>
        </Box>
    )
}

export default CommandPalette
