import { Alert, Divider, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useRef, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import World from "@/systems/World"
import FieldMiraEditor, { devtoolHandlers, type SynthesisDevtoolKey } from "../../mirabuf/FieldMiraEditor"
import { globalAddToast } from "../components/GlobalUIControls"
import type { PanelImplProps } from "../components/Panel"
import { Button } from "../components/StyledComponents"
import { useUIContext } from "../helpers/UIProviderHelpers"
import SelectMenu from "@/components/SelectMenu.tsx"
import { AssemblySelectionOption } from "@/panels/configuring/assembly-config/configure/AssemblySelection.tsx"
import { tryParse } from "@/util/Utility.ts"

const devtoolKeys = Object.keys(devtoolHandlers) as SynthesisDevtoolKey[]
const DeveloperToolPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [selectedKey, setSelectedKey] = useState<SynthesisDevtoolKey | undefined>(undefined)
    const [jsonValue, setJsonValue] = useState<string>("")
    const [error, setError] = useState<string>("")
    const [editor, setEditor] = useState<FieldMiraEditor | undefined>(undefined)
    const [keys, setKeys] = useState<string[]>([])
    const [activeObj, setActiveObj] = useState<MirabufSceneObject | undefined>(undefined)
    const prevMiraObj = useRef<MirabufSceneObject | undefined>(undefined)

    // Effect: Watch for field changes and update editor/keys only if field changes
    useEffect(() => {
        const updateEditor = () => {
            if (activeObj !== prevMiraObj.current) {
                prevMiraObj.current = activeObj
                if (activeObj) {
                    const parts = activeObj.mirabufInstance.parser.assembly.data?.parts
                    if (parts) {
                        const newEditor = new FieldMiraEditor(parts)
                        setEditor(newEditor)
                        setKeys(newEditor.getSynthesisKeys())
                    } else {
                        setEditor(undefined)
                        setKeys([])
                    }
                } else {
                    setEditor(undefined)
                    setKeys([])
                }
                setSelectedKey(undefined)
                setJsonValue("")
                setError("")
            } else if (activeObj && editor) {
                setKeys(editor.getSynthesisKeys())
            }
        }
        updateEditor()
        const interval = setInterval(updateEditor, 1000)
        return () => clearInterval(interval)
    }, [editor, activeObj])

    // Load value when key changes
    useEffect(() => {
        if (!editor || !activeObj || !selectedKey) return
        activeObj.savePreferencesToMirabuf()
        const val = devtoolHandlers[selectedKey]?.get(activeObj)
        setJsonValue(JSON.stringify(val, null, 2))
        setError("")
    }, [selectedKey, editor, activeObj])

    const handleSave = async () => {
        if (!editor || !selectedKey || !activeObj) return
        setError("")

        const parsed = tryParse(jsonValue)
        if (parsed == null) {
            setError("Invalid JSON")
            return
        }
        editor.setUserData(selectedKey, parsed)

        setKeys(editor.getSynthesisKeys())

        devtoolHandlers[selectedKey]?.set(activeObj, parsed)
    }

    const handleExport = () => {
        if (!activeObj) {
            globalAddToast?.("error", "Export Error", "No object loaded to export.")
            return
        }
        const assembly = activeObj?.mirabufInstance.parser.assembly
        if (!assembly) {
            globalAddToast?.("error", "Export Error", "No assembly found for object.")
            return
        }
        try {
            const encoded = mirabuf.Assembly.encode(assembly).finish()
            const blob = new Blob([encoded.buffer as ArrayBuffer], {
                type: "application/octet-stream",
            })
            const url = URL.createObjectURL(blob)

            const filename = `${assembly.info?.name ?? "unknown"}.mira`

            const a = document.createElement("a")
            a.href = url
            a.download = filename
            document.body.appendChild(a)
            a.click()
            setTimeout(() => {
                document.body.removeChild(a)
                URL.revokeObjectURL(url)
            }, 0)
            globalAddToast?.("info", "Exported", `Exported as ${filename}`)
        } catch (_e) {
            globalAddToast?.("error", "Export Error", "Failed to export.")
        }
    }

    useEffect(() => {
        configureScreen(panel!, { title: "Developer Tool", hideAccept: true, cancelText: "Close" }, {})
    }, [])

    return (
        <>
            <SelectMenu
                options={World.sceneRenderer.mirabufSceneObjects
                    .getAll()
                    .map(obj => new AssemblySelectionOption(obj.descriptiveName, obj))}
                onOptionSelected={val => setActiveObj((val as AssemblySelectionOption)?.assemblyObject)}
                defaultHeaderText={`Select an object`}
                noOptionsText={`Nothing spawned!`}
            />
            {editor && (
                <Stack gap={2} direction="column">
                    <Stack gap={2} direction="row">
                        {/* Key List */}
                        <Stack gap={2} className="bg-gray-700 dark:bg-gray-800 rounded-lg p-3 shadow-xs">
                            <div className="font-bold text-base mb-1 text-gray-100">Devtool Data Keys</div>
                            <ul className="list-none p-0 m-0 flex-1">
                                <div className="text-xs mb-1 text-gray-300">Current</div>
                                {keys.length === 0 && <li className="text-gray-400 italic">No devtool data</li>}
                                {keys.map(key => (
                                    <li key={key} className="mb-1">
                                        <Button
                                            onClick={() => setSelectedKey(key as SynthesisDevtoolKey)}
                                            className="w-full"
                                        >
                                            {key}
                                        </Button>
                                    </li>
                                ))}
                            </ul>
                            <Divider />
                            <div className="text-xs mb-1 text-gray-300">Available</div>
                            <ul className="list-none p-0 m-0 flex-1">
                                {devtoolKeys.filter(k => !keys.includes(k)).length === 0 && (
                                    <div className="text-gray-400 italic text-xs">All keys added</div>
                                )}
                                {devtoolKeys
                                    .filter(k => !keys.includes(k))
                                    .map(key => (
                                        <li key={key} className="mb-1">
                                            <Button
                                                onClick={() => setSelectedKey(key as SynthesisDevtoolKey)}
                                                className="w-full"
                                            >
                                                {key}
                                            </Button>
                                        </li>
                                    ))}
                            </ul>
                        </Stack>
                        {/* Editor */}
                        <div className="min-w-[360px] flex-1 bg-gray-800 dark:bg-gray-900 rounded-lg p-4 shadow-xs text-gray-100">
                            {selectedKey ? (
                                <>
                                    {/* strip off the prefix here */}
                                    <div className="font-bold text-sm mb-2">{selectedKey}</div>
                                    <textarea
                                        className={`
                            w-full h-48 font-mono text-sm
                            bg-gray-700 dark:bg-gray-800
                            border border-gray-600
                            text-gray-100
                            rounded p-2
                            resize-vertical
                            focus:outline-hidden focus:ring-2 focus:ring-blue-500
                        `}
                                        value={jsonValue}
                                        onChange={e => setJsonValue(e.target.value)}
                                        placeholder="Enter JSON data for this key"
                                    />
                                    {error && (
                                        <Alert severity="error" className="mt-2">
                                            {error}
                                        </Alert>
                                    )}
                                    <div className="mt-3 flex gap-2">
                                        <Button onClick={handleSave}>Apply</Button>
                                    </div>
                                </>
                            ) : (
                                <div className="text-gray-400 italic mt-10 text-center">Select a key to edit.</div>
                            )}
                        </div>
                    </Stack>
                    <Button onClick={handleExport}>Export</Button>
                </Stack>
            )}
        </>
    )
}

export default DeveloperToolPanel
