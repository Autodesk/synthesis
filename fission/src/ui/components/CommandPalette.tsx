import Fuse from "fuse.js"
import { Box, List, ListItemButton, ListItemText, Paper, Stack, TextField } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import DebugPanel from "@/ui/panels/DebugPanel"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import SettingsModal from "@/ui/modals/configuring/SettingsModal"
import type { PanelImplProps } from "@/ui/components/Panel"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import World from "@/systems/World"
import type { ConfigurationType } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import { loadCommandClassifier } from "@/util/useLocalAi"

const AI_FALLBACK_DEBOUNCE_TIME = 250

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

    const [aiFallbackResults, setAiFallbackResults] = useState<CommandDefinition[]>([])
    // Cache the classifier function and manage debounced re-classification on query changes
    const aiClassifyRef = useRef<null | ((q: string) => Promise<unknown>)>(null)
    const aiDebounceTimerRef = useRef<number | null>(null)
    const lastAiQueryRef = useRef<string>("")

    const closePalette = useCallback(() => {
        setIsOpen(false)
        setQuery("")
        setActiveIndex(0)
        setAiFallbackResults([])
    }, [])

    const openPalette = useCallback(() => {
        setIsOpen(true)
        setTimeout(() => inputRef.current?.focus(), 0)
    }, [])

    const openImportPanel = useCallback(
        (configurationType: ConfigurationType) => {
            openPanel<void, { configurationType: ConfigurationType }>(
                ImportMirabufPanel as unknown as React.FunctionComponent<
                    PanelImplProps<void, { configurationType: ConfigurationType }>
                >,
                { configurationType }
            )
        },
        [openPanel]
    )

    const commands = useMemo<CommandDefinition[]>(
        () => [
            {
                id: "open-debug-panel",
                label: "Open Debug Panel",
                description: "Open the Debug tools panel.",
                keywords: ["panel", "debug"],
                perform: () =>
                    openPanel(DebugPanel as unknown as React.FunctionComponent<PanelImplProps<void, void>>, undefined),
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
                id: "open-settings",
                label: "Open Settings",
                description: "Open the Settings modal.",
                keywords: ["settings", "preferences", "config"],
                perform: () =>
                    openModal(
                        SettingsModal as unknown as React.FunctionComponent<ModalImplProps<void, void>>,
                        undefined
                    ),
            },
        ],
        [addToast, openPanel, openModal, openImportPanel]
    )

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

    const visible = useMemo(() => {
        return filtered.length > 0 ? filtered.slice(0, 5) : aiFallbackResults
    }, [filtered, aiFallbackResults])

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

    useEffect(() => {
        const q = query.trim()
        // Clear any pending debounce
        if (aiDebounceTimerRef.current) {
            clearTimeout(aiDebounceTimerRef.current)
            aiDebounceTimerRef.current = null
        }

        // Reset on empty query
        if (!q) {
            setAiFallbackResults([])
            lastAiQueryRef.current = ""
            return
        }

        // If Fuse found results, prefer those and clear AI fallback
        if (filtered.length > 0) {
            setAiFallbackResults([])
            lastAiQueryRef.current = ""
            return
        }

        // Debounce AI fallback classification when no Fuse results
        aiDebounceTimerRef.current = window.setTimeout(async () => {
            try {
                console.log("Running AI fallback")
                // Build candidate strings that include both label and description for better semantic matching
                const aiCandidates = commands.map(c => `${c.label} | ${c.description ?? ""}`)
                const candidateToCommand = new Map<string, CommandDefinition>()
                aiCandidates.forEach((cand, i) => candidateToCommand.set(cand, commands[i]))

                // Load once and cache
                if (!aiClassifyRef.current) {
                    aiClassifyRef.current = await loadCommandClassifier(aiCandidates)
                }
                const classify = aiClassifyRef.current
                if (!classify) return

                const currentQuery = q
                lastAiQueryRef.current = currentQuery
                const result = (await classify(currentQuery)) as { labels?: string[]; scores?: number[] }
                console.log("AI fallback result", result)

                // Ignore stale results if query changed while awaiting
                if (lastAiQueryRef.current !== currentQuery) return

                if (!result || Array.isArray(result)) {
                    setAiFallbackResults([])
                    return
                }
                const labels = result.labels ?? []
                const scores = result.scores ?? []
                if (labels.length === 0) {
                    setAiFallbackResults([])
                    return
                }
                // Build candidates with scores (if present), filter by score >= 0.5, sort desc, take top 5
                const candidates = labels.map((lab, i) => ({ label: lab, score: scores[i] }))
                const eligible = candidates
                    .filter(c => typeof c.score === "number" && (c.score as number) >= 0.3)
                    .sort((a, b) => (b.score as number) - (a.score as number))
                    .slice(0, 5)

                if (eligible.length === 0) {
                    const fallbackLabel = labels[0]
                    if (!fallbackLabel) {
                        setAiFallbackResults([])
                        return
                    }
                    const fallbackCmd = candidateToCommand.get(fallbackLabel)
                    setAiFallbackResults(fallbackCmd ? [fallbackCmd] : [])
                } else {
                    const topMatches: CommandDefinition[] = []
                    for (const c of eligible) {
                        const cmd = candidateToCommand.get(c.label)
                        if (cmd) topMatches.push(cmd)
                    }
                    setAiFallbackResults(topMatches.reverse())
                }
            } catch (err) {
                console.warn("AI fallback failed", err)
                setAiFallbackResults([])
            }
        }, AI_FALLBACK_DEBOUNCE_TIME)

        return () => {
            if (aiDebounceTimerRef.current) {
                clearTimeout(aiDebounceTimerRef.current)
                aiDebounceTimerRef.current = null
            }
        }
    }, [query, filtered, commands])

    const onInputKeyDown = useCallback(
        (e: React.KeyboardEvent<HTMLInputElement>) => {
            if (e.key === "ArrowDown") {
                e.preventDefault()
                setActiveIndex(i => Math.min(i + 1, Math.max(visible.length - 1, 0)))
            } else if (e.key === "ArrowUp") {
                e.preventDefault()
                setActiveIndex(i => Math.max(i - 1, 0))
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
