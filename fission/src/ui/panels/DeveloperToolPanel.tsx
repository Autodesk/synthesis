import Panel, { PanelPropsImpl } from "../components/Panel"
import { SynthesisIcons } from "../components/StyledComponents"
import React, { useState, useEffect, useRef } from "react"
import FieldMiraEditor from "../../mirabuf/FieldMiraEditor"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import Button, { ButtonSize } from "../components/Button"

const DEVTOOL_KEYS = [
    "devtool:scoring_zones",
    "devtool:spawn_points",
    "devtool:camera_locations",
]

function getCurrentFieldObj() {
    for (const obj of World.SceneRenderer.sceneObjects.values()) {
        if (obj instanceof MirabufSceneObject && obj.miraType === MiraType.FIELD) {
            return obj
        }
    }
    return undefined
}

const DeveloperToolPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel } = usePanelControlContext()
    const [selectedKey, setSelectedKey] = useState<string | undefined>(undefined)
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
        updateEditor()
        const interval = setInterval(updateEditor, 1000)
        return () => clearInterval(interval)
    }, [editor])

    // Load value when key changes
    useEffect(() => {
        if (editor && selectedKey) {
            const val = editor.getUserData(selectedKey)
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

    const handleAdd = (key: string) => {
        setSelectedKey(key)
        setJsonValue("{}")
        setError("")
    }

    const handleAccept = () => closePanel(panelId)
    const handleCancel = () => closePanel(panelId)

    const buttonSize = ButtonSize.Small

    return (
        <Panel
            name={"Developer Tool"}
            icon={SynthesisIcons.CodeSquare}
            panelId={panelId}
            acceptEnabled={true}
            cancelEnabled={true}
            onAccept={handleAccept}
            onCancel={handleCancel}
            acceptName="Close"
            cancelName="Cancel"
            openLocation="right"
        >
            <div className="flex flex-col gap-4 bg-background-secondary rounded-md p-4 max-h-[60vh] min-h-[350px] overflow-y-auto">
                {!fieldLoaded && <div className="text-red-600 m-4">No mira field loaded.</div>}
                {editor && (
                    <div className="flex flex-row gap-6 items-start">
                        {/* Key List */}
                        <div className="min-w-[220px] bg-gray-100 rounded-lg p-3 shadow-sm flex flex-col gap-2">
                            <div className="font-bold text-base mb-1">Devtool Data Keys</div>
                            <ul className="list-none p-0 m-0 flex-1">
                                {keys.length === 0 && <li className="text-gray-400 italic">No devtool data</li>}
                                {keys.map(key => (
                                    <li key={key} className="mb-1">
                                        <button
                                            onClick={() => setSelectedKey(key)}
                                            className={`w-full text-left px-2 py-1 rounded ${selectedKey === key ? "bg-blue-100 font-bold" : "hover:bg-gray-200"}`}
                                        >
                                            {key}
                                        </button>
                                    </li>
                                ))}
                            </ul>
                            <div className="mt-2 border-t border-gray-200 pt-2">
                                <div className="text-xs mb-1">Add new:</div>
                                {DEVTOOL_KEYS.filter(k => !keys.includes(k)).map(key => (
                                    <Button key={key} onClick={() => handleAdd(key)} className="w-full mb-1" size={buttonSize} value={key} />
                                ))}
                                {DEVTOOL_KEYS.filter(k => !keys.includes(k)).length === 0 && (
                                    <div className="text-gray-400 italic text-xs">All keys added</div>
                                )}
                            </div>
                        </div>
                        {/* Editor */}
                        <div className="flex-1 bg-white rounded-lg p-4 shadow-sm">
                            {selectedKey ? (
                                <>
                                    <div className="font-bold text-sm mb-2">{selectedKey}</div>
                                    <textarea
                                        className="w-full h-48 font-mono text-sm bg-gray-50 border border-gray-300 rounded p-2 resize-vertical"
                                        value={jsonValue}
                                        onChange={e => setJsonValue(e.target.value)}
                                        placeholder="Enter JSON data for this key"
                                    />
                                    {error && <div className="text-red-600 mt-1">{error}</div>}
                                    <div className="mt-3 flex gap-2">
                                        <Button onClick={handleSave} size={buttonSize} value="Save" />
                                        <Button onClick={handleRemove} size={buttonSize} value="Remove" />
                                    </div>
                                </>
                            ) : (
                                <div className="text-gray-400 italic mt-10 text-center">Select a key to edit or add a new one.</div>
                            )}
                        </div>
                    </div>
                )}
            </div>
        </Panel>
    )
}

export default DeveloperToolPanel