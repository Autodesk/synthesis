import { Alert, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useRef, useState } from "react"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World"
import FieldMiraEditor, { type DevtoolKey, devtoolHandlers, devtoolKeys } from "../../mirabuf/FieldMiraEditor"
import { globalAddToast } from "../components/GlobalUIControls"
import type { PanelImplProps } from "../components/Panel"
import { Button, LabelWithTooltip } from "../components/StyledComponents"
import { useUIContext } from "../helpers/UIProviderHelpers"

async function saveToCache() {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) return
    const assembly = field.mirabufInstance.parser.assembly
    const newName = assembly.info?.name != null ? `Edited ${assembly.info.name}` : undefined
    const existing = MirabufCachingService.getAll().find(info => info.name == newName)
    const cacheInfo = await MirabufCachingService.storeAssemblyInCache(assembly, {
        miraType: MiraType.FIELD,
        name: newName,
    })

    if (cacheInfo != null) {
        globalAddToast("info", "Devtool Saved", "Changes have been persisted to cache.")
    } else {
        globalAddToast("warning", "Devtool Warning", "Changes saved but failed to persist to cache.")
    }

    if (existing) {
        await MirabufCachingService.remove(existing.hash)
    }
}

const DeveloperToolPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [selectedKey, setSelectedKey] = useState<DevtoolKey | undefined>(undefined)
    const [jsonValue, setJsonValue] = useState<string>("")
    const [error, setError] = useState<string>("")
    const [editor, setEditor] = useState<FieldMiraEditor | undefined>(undefined)
    const [keys, setKeys] = useState<string[]>([])
    const [fieldLoaded, setFieldLoaded] = useState<boolean>(false)
    const prevFieldObj = useRef<MirabufSceneObject | undefined>(undefined)

    // Effect: Watch for field changes and update editor/keys only if field changes
    useEffect(() => {
        const updateEditor = () => {
            const currentField = World.sceneRenderer.mirabufSceneObjects.getField()
            if (currentField !== prevFieldObj.current) {
                prevFieldObj.current = currentField
                if (currentField) {
                    const parts = currentField.mirabufInstance.parser.assembly.data?.parts
                    if (parts) {
                        const newEditor = new FieldMiraEditor(parts)
                        setEditor(newEditor)
                        setKeys(newEditor.getAllDevtoolKeys())
                        setFieldLoaded(true)
                    } else {
                        setEditor(undefined)
                        setKeys([])
                        setFieldLoaded(false)
                    }
                } else {
                    setEditor(undefined)
                    setKeys([])
                    setFieldLoaded(false)
                }
                setSelectedKey(undefined)
                setJsonValue("")
                setError("")
            } else if (currentField && editor) {
                setKeys(editor.getAllDevtoolKeys())
            }
        }
        const allKeys = editor?.getAllDevtoolKeys()
        console.log("devtool keys (poll):", allKeys)

        updateEditor()
        const interval = setInterval(updateEditor, 1000)
        return () => clearInterval(interval)
    }, [editor])

    // Load value when key changes
    useEffect(() => {
        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        if (!editor || !field || !selectedKey) return

        const val = devtoolHandlers[selectedKey].get(field)
        editor.setUserData(selectedKey, val)
        setJsonValue(JSON.stringify(val, null, 2))
        setError("")
    }, [selectedKey, editor])

    const handleSave = async () => {
        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        if (!editor || !selectedKey || !field) return
        try {
            setError("")
            const parsed = JSON.parse(jsonValue) as unknown
            if (!devtoolHandlers[selectedKey].validate(parsed)) {
                setError("Value does not match required format")
                return
            }
            editor.setUserData(selectedKey, parsed)

            setKeys(editor.getAllDevtoolKeys())

            // Persist changes to cache
            await saveToCache()

            if (!field.fieldPreferences) {
                globalAddToast?.("error", "Devtool Error", "Field preferences not available.")
                return
            }

            devtoolHandlers[selectedKey].set(field, parsed)
            PreferencesSystem.savePreferences?.()
        } catch (_e) {
            setError("Invalid JSON")
        }
    }

    const handleRemove = async () => {
        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        if (!editor || !selectedKey || !field) return

        editor.removeUserData(selectedKey)
        setKeys(editor.getAllDevtoolKeys())
        setSelectedKey(undefined)
        setJsonValue("")
        setError("")

        // Persist removal to cache
        await saveToCache()

        if (!field.fieldPreferences) return

        devtoolHandlers[selectedKey].set(field, null)
        PreferencesSystem.savePreferences?.()
    }

    const handleAdd = (key: DevtoolKey) => {
        setSelectedKey(key)
        setJsonValue("{}")
        setError("")
    }

    const handleExport = () => {
        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        if (!field) {
            globalAddToast?.("error", "Export Error", "No field loaded to export.")
            return
        }
        const assembly = field?.mirabufInstance.parser.assembly
        if (!assembly) {
            globalAddToast?.("error", "Export Error", "No assembly found for field.")
            return
        }
        try {
            const encoded = mirabuf.Assembly.encode(assembly).finish()
            const blob = new Blob([encoded.buffer as ArrayBuffer], {
                type: "application/octet-stream",
            })
            const url = URL.createObjectURL(blob)

            // Check if assembly has devtool data to determine filename
            let name = assembly.info?.name ?? "field"
            if (assembly.data?.parts?.userData?.data) {
                const devtoolKeys = Object.keys(assembly.data.parts.userData.data).filter(k => k.startsWith("devtool:"))
                if (devtoolKeys.length > 0) {
                    name = `edited-${name}`
                }
            }
            const filename = `${name}.mira`

            const a = document.createElement("a")
            a.href = url
            a.download = filename
            document.body.appendChild(a)
            a.click()
            setTimeout(() => {
                document.body.removeChild(a)
                URL.revokeObjectURL(url)
            }, 0)
            globalAddToast?.("info", "Exported", `Exported field as ${filename}`)
        } catch (_e) {
            globalAddToast?.("error", "Export Error", "Failed to export field.")
        }
    }

    useEffect(() => {
        configureScreen(panel!, { title: "Developer Tool", acceptText: "Exit", hideCancel: true }, {})
    }, [])

    return (
        <Stack gap={4} className="rounded-md p-4 max-h-[60vh] overflow-y-auto">
            {!fieldLoaded && (
                <Alert severity="warning" className="m-2">
                    No mira field loaded.
                </Alert>
            )}
            {editor && (
                <Stack gap={6} className="md:flex-row items-start">
                    {/* Key List */}
                    <Stack gap={2} className="min-w-[220px] bg-gray-700 dark:bg-gray-800 rounded-lg p-3 shadow-xs">
                        <div className="font-bold text-base mb-1 text-gray-100">Devtool Data Keys</div>
                        <ul className="list-none p-0 m-0 flex-1">
                            {keys.length === 0 && <li className="text-gray-400 italic">No devtool data</li>}
                            {keys.map(key => (
                                <li key={key} className="mb-1">
                                    <Button
                                        onClick={() => setSelectedKey(key as DevtoolKey)}
                                        className={`
                            w-full whitespace-normal break-words text-left
                            px-2 py-1 rounded
                            ${
                                selectedKey === key
                                    ? "bg-blue-600 text-white font-bold"
                                    : "bg-gray-700 text-gray-100 hover:bg-gray-600"
                            }
                            `}
                                    >
                                        {key}
                                    </Button>
                                </li>
                            ))}
                        </ul>
                        <div className="mt-2 border-t border-gray-600 pt-2">
                            <div className="text-xs mb-1 text-gray-300">Add new:</div>
                            {devtoolKeys
                                .filter(k => !keys.includes(k))
                                .map(key => (
                                    <Button
                                        key={key}
                                        onClick={() => handleAdd(key)}
                                        className="w-full mb-1 whitespace-normal break-words"
                                    >
                                        {key}
                                    </Button>
                                ))}
                            {devtoolKeys.filter(k => !keys.includes(k)).length === 0 && (
                                <div className="text-gray-400 italic text-xs">All keys added</div>
                            )}
                        </div>
                    </Stack>
                    {/* Editor */}
                    <div className="min-w-[360px] flex-1 bg-gray-800 dark:bg-gray-900 rounded-lg p-4 shadow-xs text-gray-100">
                        {selectedKey ? (
                            <>
                                {/* strip off the prefix here */}
                                {selectedKey === "devtool:scoring_zones" ? (
                                    LabelWithTooltip(
                                        "scoring_zones",
                                        'Add and cache scoring zones. \n Example:\n[\n  {\n    "name": "Red Zone",\n    "alliance": "red",\n    "parentNode": "root",\n    "points": 5,\n    "destroyGamepiece": false,\n    "persistentPoints": true,\n    "deltaTransformation": [1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1]\n  }\n]'
                                    )
                                ) : (
                                    <div className="font-bold text-sm mb-2">{selectedKey.replace(/^devtool:/, "")}</div>
                                )}
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
                                    <Button onClick={handleSave}>Save</Button>
                                    <Button onClick={handleRemove}>Remove</Button>
                                    <Button onClick={handleExport}>Export</Button>
                                </div>
                            </>
                        ) : (
                            <div className="text-gray-400 italic mt-10 text-center">
                                Select a key to edit or add a new one.
                            </div>
                        )}
                    </div>
                </Stack>
            )}
        </Stack>
    )
}

export default DeveloperToolPanel
