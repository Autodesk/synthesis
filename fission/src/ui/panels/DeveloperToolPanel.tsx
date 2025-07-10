import Panel, { PanelPropsImpl } from "../components/Panel"
import { SynthesisIcons } from "../components/StyledComponents"
import React, { useState, useEffect, useRef } from "react"
import FieldMiraEditor from "../../mirabuf/FieldMiraEditor"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import Button, { ButtonSize } from "../components/Button"
import { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { globalAddToast } from "../components/GlobalUIControls"
import { LabelWithTooltip } from "../components/StyledComponents"

const DEVTOOL_KEYS = ["devtool:scoring_zones", "devtool:spawn_points", "devtool:camera_locations"] as const
type DevtoolKey = (typeof DEVTOOL_KEYS)[number]

function getCurrentFieldObj() {
    for (const obj of World.sceneRenderer.sceneObjects.values()) {
        if (obj instanceof MirabufSceneObject && obj.miraType === MiraType.FIELD) {
            return obj
        }
    }
    return undefined
}

// Helper: type guard for ScoringZonePreferences[]
function isScoringZonePreferencesArray(val: unknown): val is ScoringZonePreferences[] {
    if (!Array.isArray(val)) return false
    return val.every(
        z =>
            typeof z === "object" &&
            z !== null &&
            typeof z.name === "string" &&
            (z.alliance === "red" || z.alliance === "blue") &&
            (typeof z.parentNode === "string" || z.parentNode === undefined) &&
            typeof z.points === "number" &&
            typeof z.destroyGamepiece === "boolean" &&
            typeof z.persistentPoints === "boolean" &&
            Array.isArray(z.deltaTransformation)
    )
}

const DeveloperToolPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel } = usePanelControlContext()
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
            const currentField = getCurrentFieldObj()
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
        if (editor && selectedKey) {
            const val = editor.getUserData(selectedKey)
            // console.log(val ? JSON.stringify(val, null, 2) : "")
            setJsonValue(val ? JSON.stringify(val, null, 2) : "")
            setError("")
        }
    }, [selectedKey, editor])

    const handleSave = () => {
        if (!editor || !selectedKey) return
        try {
            const parsed = JSON.parse(jsonValue)
            editor.setUserData(selectedKey, parsed)
            setError("")
            setKeys(editor.getAllDevtoolKeys())

            if (selectedKey === "devtool:scoring_zones") {
                const field = getCurrentFieldObj()
                if (!field) {
                    globalAddToast?.("error", "Devtool Error", "No field loaded to apply scoring zones.")
                    return
                }
                if (!isScoringZonePreferencesArray(parsed)) {
                    globalAddToast?.("error", "Devtool Error", "Value must be an array of scoring zone objects.")
                    return
                }
                if (!field.fieldPreferences) {
                    globalAddToast?.("error", "Devtool Error", "Field preferences not available.")
                    return
                }
                field.fieldPreferences.scoringZones = parsed
                PreferencesSystem.savePreferences?.() 
                field.updateScoringZones()
            }
        } catch (e) {
            setError("Invalid JSON")
        }
    }

    const handleRemove = () => {
        if (!editor || !selectedKey) return
        editor.removeUserData(selectedKey)
        setKeys(editor.getAllDevtoolKeys())
        setSelectedKey(undefined)
        setJsonValue("")
        setError("")
    }

    const handleAdd = (key: DevtoolKey) => {
        setSelectedKey(key)
        setJsonValue("{}")
        setError("")
    }

    const handleAccept = () => closePanel(panelId)
    const handleCancel = () => closePanel(panelId)

    const buttonSize = ButtonSize.SMALL

    return (
        <Panel
            name="Developer Tool"
            icon={SynthesisIcons.CODE_SQUARE}
            panelId={panelId}
            acceptEnabled={true}
            cancelEnabled={true}
            onAccept={handleAccept}
            onCancel={handleCancel}
            acceptName="Save"
            cancelName="Cancel"
            openLocation="right"
        >
            <div className="flex flex-col gap-4 bg-background-secondary rounded-md p-4 max-h-[60vh] min-h-[350px] overflow-y-auto">
                {!fieldLoaded && <div className="text-red-600 m-4">No mira field loaded.</div>}
                {editor && (
                    <div className="flex flex-col md:flex-row gap-6 items-start">
                        {/* Key List */}
                        <div className="min-w-[220px] bg-gray-700 dark:bg-gray-800 rounded-lg p-3 shadow-sm flex flex-col gap-2">
                            <div className="font-bold text-base mb-1 text-gray-100">Devtool Data Keys</div>
                            <ul className="list-none p-0 m-0 flex-1">
                                {keys.length === 0 && <li className="text-gray-400 italic">No devtool data</li>}
                                {keys.map(key => (
                                    <li key={key} className="mb-1">
                                        <button
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
                                        </button>
                                    </li>
                                ))}
                            </ul>
                            <div className="mt-2 border-t border-gray-600 pt-2">
                                <div className="text-xs mb-1 text-gray-300">Add new:</div>
                                {DEVTOOL_KEYS.filter(k => !keys.includes(k)).map(key => (
                                    <Button
                                        key={key}
                                        onClick={() => handleAdd(key)}
                                        className="w-full mb-1 whitespace-normal break-words"
                                        size={buttonSize}
                                        value={key}
                                    />
                                ))}
                                {DEVTOOL_KEYS.filter(k => !keys.includes(k)).length === 0 && (
                                    <div className="text-gray-400 italic text-xs">All keys added</div>
                                )}
                            </div>
                        </div>
                        {/* Editor */}
                        <div className="min-w-[360px] flex-1 bg-gray-800 dark:bg-gray-900 rounded-lg p-4 shadow-sm text-gray-100">
                            {selectedKey ? (
                                <>
                                    {/* strip off the prefix here */}
                                    {selectedKey === "devtool:scoring_zones"
                                        ? LabelWithTooltip(
                                              "scoring_zones",
                                              `Example:\n[\n  {\n    \"name\": \"Red Zone\",\n    \"alliance\": \"red\",\n    \"parentNode\": \"root\",\n    \"points\": 5,\n    \"destroyGamepiece\": false,\n    \"persistentPoints\": true,\n    \"deltaTransformation\": [1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1]\n  }\n]`,
                                              undefined
                                          )
                                        : <div className="font-bold text-sm mb-2">{selectedKey.replace(/^devtool:/, "")}</div>
                                    }
                                    <textarea
                                        className={`
                            w-full h-48 font-mono text-sm
                            bg-gray-700 dark:bg-gray-800
                            border border-gray-600
                            text-gray-100
                            rounded p-2
                            resize-vertical
                            focus:outline-none focus:ring-2 focus:ring-blue-500
                        `}
                                        value={jsonValue}
                                        onChange={e => setJsonValue(e.target.value)}
                                        placeholder="Enter JSON data for this key"
                                    />
                                    {error && <div className="text-red-400 mt-1">{error}</div>}
                                    <div className="mt-3 flex gap-2">
                                        <Button onClick={handleSave} size={buttonSize} value="Save" />
                                        <Button onClick={handleRemove} size={buttonSize} value="Remove" />
                                    </div>
                                </>
                            ) : (
                                <div className="text-gray-400 italic mt-10 text-center">
                                    Select a key to edit or add a new one.
                                </div>
                            )}
                        </div>
                    </div>
                )}
            </div>
        </Panel>
    )
}

export default DeveloperToolPanel
