import type React from "react"
import { useEffect, useState } from "react"
import { CameraMode, type CustomTargetControls } from "@/systems/scene/CameraControls"
import EventSystem from "@/systems/EventSystem"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Select, ToggleButtonGroup, TooltipToggleButton } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import CommandRegistry from "@/ui/components/CommandRegistry"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"
import { MenuItem } from "@mui/material"

interface TargetSettingsProps {
    controls: CustomTargetControls
}

CommandRegistry.get().registerCommand({
    id: "open-camera-config",
    label: "Open Camera Configuration",
    description: "Open the Camera Config panel",
    keywords: ["camera", "config", "target", "follow", "locked", "face"],
    perform: () => import("./CameraSelectionPanel").then(m => globalOpenPanel(m.default, undefined)),
})

const UNFOCUSED_ID = -1

function getFocusTargets(): MirabufSceneObject[] {
    const robots = World.sceneRenderer.mirabufSceneObjects.getRobots()
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    return [...robots, ...(field ? [field] : [])]
}

const FocusSelector: React.FC<{ controls: CustomTargetControls }> = ({ controls }) => {
    const [targets, setTargets] = useState<MirabufSceneObject[]>(getFocusTargets)
    const [focusedId, setFocusedId] = useState<number>(controls.focusProvider?.id ?? UNFOCUSED_ID)

    useEffect(() => {
        return EventSystem.listen("MirabufObjectChangeEvent", () => {
            setTargets(getFocusTargets())
        })
    }, [])

    useEffect(() => {
        return EventSystem.listen("CameraFocusChangedEvent", ({ focusProvider }) => {
            setFocusedId(focusProvider?.id ?? UNFOCUSED_ID)
        })
    }, [])

    return (
        <div className="flex flex-col gap-1 w-full">
            <span className="text-xs opacity-70">Focus Target</span>
            <Select
                value={focusedId}
                onChange={e => {
                    const id = e.target.value as number
                    if (id === UNFOCUSED_ID) {
                        controls.unfocus()
                    } else {
                        const target = targets.find(t => t.id === id)
                        if (target) controls.focusProvider = target
                    }
                }}
                size="small"
                fullWidth
            >
                <MenuItem value={UNFOCUSED_ID}>None</MenuItem>
                {targets.map(t => (
                    <MenuItem key={t.id} value={t.id}>
                        {t.assemblyName}
                    </MenuItem>
                ))}
            </Select>
        </div>
    )
}

const TargetSettings: React.FC<TargetSettingsProps> = ({ controls }) => {
    const [mode, setMode] = useState<CameraMode>(controls.mode)
    const [focusedOnField, setFocusedOnField] = useState<boolean>(controls.isFocusedOnField)

    useEffect(() => {
        return EventSystem.listen("CameraModeChangedEvent", ({ mode: newMode }) => {
            setMode(newMode as CameraMode)
        })
    }, [])

    useEffect(() => {
        return EventSystem.listen("CameraFocusChangedEvent", ({ focusProvider }) => {
            setFocusedOnField(focusProvider?.miraType === MiraType.FIELD)
        })
    }, [])

    useEffect(() => {
        controls.mode = mode
    }, [controls, mode])

    return (
        <ToggleButtonGroup
            orientation="horizontal"
            value={mode}
            exclusive
            onChange={(_, v) => {
                if (v !== null) setMode(v as CameraMode)
            }}
        >
            <TooltipToggleButton title="Follow the target position, but allow free rotation" value={CameraMode.Follow}>
                Follow
            </TooltipToggleButton>
            <TooltipToggleButton title="Follow the target with camera position and rotation" value={CameraMode.Locked}>
                Locked
            </TooltipToggleButton>
            <TooltipToggleButton
                title="Lock camera position and orient the camera to face the target"
                value={CameraMode.Face}
                disabled={focusedOnField}
            >
                Face
            </TooltipToggleButton>
        </ToggleButtonGroup>
    )
}

const CameraSelectionPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    // const [cameraControlType, setCameraControlType] = useState<CameraControlsType>(
    //     World.sceneRenderer.currentCameraControls.controlsType
    // )

    // TODO add toggle button groups once more control types are available
    // const setCameraControls = useCallback((t: CameraControlsType) => {
    //     switch (t) {
    //         case "Target":
    //             World.sceneRenderer.setCameraControls(t)
    //             setCameraControlType(t)
    //             break
    //         default:
    //             console.error("Unrecognized camera control option detected")
    //             break
    //     }
    // }, [])

    useEffect(() => {
        configureScreen(panel!, { title: "Camera Config", hideAccept: true, cancelText: "Close" }, {})
    }, [])

    return (
        <div className="flex flex-col gap-2">
            <FocusSelector controls={World.sceneRenderer.currentCameraControls as CustomTargetControls} />
            <TargetSettings controls={World.sceneRenderer.currentCameraControls as CustomTargetControls} />
        </div>
    )
}

export default CameraSelectionPanel
