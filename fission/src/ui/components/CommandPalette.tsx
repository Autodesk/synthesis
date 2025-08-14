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
    const { addToast, openPanel, openModal } = useUIContext()
    const { isMainMenuOpen } = useStateContext()

    const [isOpen, setIsOpen] = useState<boolean>(false)
    const [query, setQuery] = useState<string>("")
    const [activeIndex, setActiveIndex] = useState<number>(0)
    const inputRef = useRef<HTMLInputElement | null>(null)

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
                id: "toast-info",
                label: "Toast: Show info",
                description: "Show an info toast in the bottom-right.",
                keywords: ["notification", "snackbar", "message"],
                perform: () => addToast("info", "Hello from Command Palette"),
            },
            {
                id: "toast-success",
                label: "Toast: Show success",
                description: "Show a success toast in the bottom-right.",
                keywords: ["notification", "snackbar", "message"],
                perform: () => addToast("success", "Success!"),
            },
            {
                id: "open-debug-panel",
                label: "Open Debug Panel",
                description: "Open the Debug tools panel.",
                keywords: ["panel", "debug"],
                perform: () =>
                    openPanel(DebugPanel as unknown as React.FunctionComponent<PanelImplProps<void, void>>, undefined),
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

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase()
        if (q.length === 0) return commands

        type Scored = { cmd: CommandDefinition; score: number; idx: number }
        const scored: Scored[] = []
        for (let i = 0; i < commands.length; i++) {
            const c = commands[i]
            const label = c.label.toLowerCase()
            const keywords = (c.keywords ?? []).map(k => k.toLowerCase())
            let score = 0
            if (label === q) score += 1000
            if (label.startsWith(q)) score += 500
            if (label.includes(q)) score += 200
            if (keywords.includes(q)) score += 300
            if (keywords.some(k => k.startsWith(q))) score += 150
            if (keywords.some(k => k.includes(q))) score += 50
            if (score > 0) {
                scored.push({ cmd: c, score, idx: i })
            }
        }
        scored.sort((a, b) => (a.score === b.score ? a.idx - b.idx : a.score - b.score))
        return scored.map(s => s.cmd)
    }, [commands, query])

    const visible = useMemo(() => filtered.slice(0, 5), [filtered])

    const execute = useCallback(
        (index: number) => {
            const cmd = visible[index]
            cmd.perform()
            setIsOpen(false)
            setQuery("")
            setActiveIndex(0)
        },
        [visible]
    )

    useEffect(() => {
        const onKeyDown = (e: KeyboardEvent) => {
            if (e.key === "/") {
                if (isTextInputTarget(e.target)) return
                if (!World.isAlive) return
                if (isMainMenuOpen) return
                e.preventDefault()
                setIsOpen(true)
                setTimeout(() => inputRef.current?.focus(), 0)
            } else if (e.key === "Escape") {
                if (isOpen) {
                    e.preventDefault()
                    setIsOpen(false)
                    setQuery("")
                    setActiveIndex(0)
                }
            }
        }
        window.addEventListener("keydown", onKeyDown)
        return () => window.removeEventListener("keydown", onKeyDown)
    }, [isOpen, isMainMenuOpen])

    useEffect(() => {
        if (isMainMenuOpen && isOpen) {
            setIsOpen(false)
            setQuery("")
            setActiveIndex(0)
        }
    }, [isMainMenuOpen, isOpen])

    useEffect(() => {
        if (!isOpen) return
        setActiveIndex(visible.length > 0 ? visible.length - 1 : 0)
    }, [isOpen, visible.length])

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
                setIsOpen(false)
                setQuery("")
                setActiveIndex(0)
            }
        },
        [activeIndex, execute, visible.length]
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
                <Paper elevation={8} sx={{ width: "min(800px, 95vw)" }}>
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
