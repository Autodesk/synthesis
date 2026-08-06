import type React from "react"
import { useEffect, useLayoutEffect, useState } from "react"
import {
    type CameraControlsType,
    CameraMode,
    CustomFieldViewControls,
    CustomTargetControls,
    getTargetControls,
} from "@/systems/scene/CameraControls"
import type { CameraPoint } from "@/systems/preferences/PreferenceTypes"
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
import type { SceneObjectId } from "@/systems/scene/SceneRenderer.ts"

CommandRegistry.get().registerCommand({
    id: "open-camera-config",
    label: "Open Camera Configuration",
    description: "Open the Camera Config panel",
    keywords: ["camera", "config", "target", "follow", "locked", "face"],
    perform: () => import("./CameraSelectionPanel").then(m => globalOpenPanel(m.default, undefined)),
})

const CENTER_POINT_INDEX = -1

function getSceneObjects(): MirabufSceneObject[] {
    const robots = World.sceneRenderer.mirabufSceneObjects.getRobots()
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    return [...robots, ...(field ? [field] : [])]
}

function getCameraPoints(): CameraPoint[] {
    return World.sceneRenderer.mirabufSceneObjects.getField()?.fieldPreferences?.cameraPoints ?? []
}

/** Follow / Locked / Face mode toggle, shown when a robot is focused. */
const TargetSettings: React.FC<{ controls: CustomTargetControls }> = ({ controls }) => {
    const [mode, setMode] = useState<CameraMode>(controls.mode)

    useEffect(() => EventSystem.listen("CameraModeChangedEvent", ({ mode: m }) => setMode(m as CameraMode)), [])

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
            <TooltipToggleButton title="Follow the target position, but allow free rotation" value={CameraMode.FOLLOW}>
                Follow
            </TooltipToggleButton>
            <TooltipToggleButton title="Follow the target with camera position and rotation" value={CameraMode.LOCKED}>
                Locked
            </TooltipToggleButton>
            <TooltipToggleButton
                title="Lock camera position and orient the camera to face the target"
                value={CameraMode.FACE}
            >
                Face
            </TooltipToggleButton>
        </ToggleButtonGroup>
    )
}

const getFieldViewControls = () => {
    const c = World.sceneRenderer.currentCameraControls
    return c instanceof CustomFieldViewControls ? c : undefined
}

/** Station / robot-focus dropdowns, shown when a field is focused. */
const FieldViewSettings: React.FC = () => {
    const [points, setPoints] = useState<CameraPoint[]>(getCameraPoints)
    const [robots, setRobots] = useState<MirabufSceneObject[]>(() =>
        World.sceneRenderer.mirabufSceneObjects.getRobots()
    )
    const [activePointIndex, setActivePointIndex] = useState<number>(() => {
        const selected = getFieldViewControls()?.selectedPoint
        return selected ? getCameraPoints().indexOf(selected) : CENTER_POINT_INDEX
    })
    const [focusedRobotId, setFocusedRobotId] = useState<SceneObjectId | null>(
        getFieldViewControls()?.focusedRobot?.id ?? null
    )

    // TODO: this is very bad react, we should not be updating this every re-render
    const refreshPoints = () => {
        const freshPoints = getCameraPoints()
        setPoints(freshPoints)
        setActivePointIndex(prev => {
            if (prev !== CENTER_POINT_INDEX && freshPoints[prev] === undefined) return CENTER_POINT_INDEX
            return prev
        })
    }

    useEffect(
        () =>
            EventSystem.listen("MirabufObjectChangeEvent", () => {
                refreshPoints()
                const freshRobots = World.sceneRenderer.mirabufSceneObjects.getRobots()
                setRobots(freshRobots)
                setFocusedRobotId(prev => {
                    if (prev !== null && !freshRobots.find(r => r.id === prev)) {
                        getFieldViewControls()?.focusRobot(undefined)
                        return null
                    }
                    return prev
                })
            }),
        []
    )

    useEffect(() => EventSystem.listen("ConfigurationSavedEvent", refreshPoints), [])

    useEffect(
        () =>
            EventSystem.listen("CameraViewChangedEvent", ({ point, focusedRobotId }) => {
                setActivePointIndex(point ? getCameraPoints().indexOf(point) : CENTER_POINT_INDEX)
                setFocusedRobotId(focusedRobotId ?? null)
            }),
        []
    )

    const selectView = (index: number) => {
        if (index === CENTER_POINT_INDEX) {
            World.sceneRenderer.setCameraControls("Target")
            const tc = World.sceneRenderer.currentCameraControls
            if (tc instanceof CustomTargetControls) {
                const field = World.sceneRenderer.mirabufSceneObjects.getField()
                if (field) tc.focusProvider = field
            }
            setActivePointIndex(CENTER_POINT_INDEX)
            setFocusedRobotId(null)
        } else {
            const point = points[index]
            const field = World.sceneRenderer.mirabufSceneObjects.getField()
            if (point && field) {
                World.sceneRenderer.setCameraControls("FieldView")
                ;(World.sceneRenderer.currentCameraControls as CustomFieldViewControls).selectPoint(field, index)
            }
        }
    }

    return (
        <div className="flex flex-col gap-2 w-full">
            <div className="flex flex-col gap-1 w-full">
                <span className="text-xs opacity-70 select-none">Station</span>
                <Select
                    value={activePointIndex}
                    onChange={e => selectView(e.target.value as number)}
                    size="small"
                    fullWidth
                >
                    <MenuItem value={CENTER_POINT_INDEX}>Center (Free Orbit)</MenuItem>
                    {points.map((p, i) => (
                        <MenuItem key={i} value={i}>
                            {p.name}
                        </MenuItem>
                    ))}
                </Select>
            </div>
            {activePointIndex !== CENTER_POINT_INDEX && (
                <div className="flex flex-col gap-1 w-full">
                    <span className="text-xs opacity-70 select-none">Focus Robot</span>
                    <Select
                        value={focusedRobotId}
                        onChange={e => {
                            const id = e.target.value as SceneObjectId
                            const robot = id == null ? undefined : robots.find(r => r.id === id)
                            setFocusedRobotId(id)
                            getFieldViewControls()?.focusRobot(robot)
                        }}
                        size="small"
                        fullWidth
                    >
                        <MenuItem value={undefined}>None</MenuItem>
                        {robots.map(r => (
                            <MenuItem key={r.id} value={r.id}>
                                {r.descriptiveName}
                            </MenuItem>
                        ))}
                    </Select>
                </div>
            )}
        </div>
    )
}

const CameraSelectionPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()

    const [focusedId, setFocusedId] = useState<SceneObjectId | null>(() => {
        const controls = World.sceneRenderer?.currentCameraControls
        if (controls instanceof CustomTargetControls) return controls.focusProvider?.id ?? null
        if (controls instanceof CustomFieldViewControls) {
            return World.sceneRenderer.mirabufSceneObjects.getField()?.id ?? null
        }
        return null
    })
    const [sceneObjects, setSceneObjects] = useState<MirabufSceneObject[]>(getSceneObjects)
    const [controlsType, setControlsType] = useState<CameraControlsType>(
        () => World.sceneRenderer?.currentCameraControls?.controlsType ?? "Target"
    )

    const focusedObject = sceneObjects.find(o => o.id === focusedId)

    useLayoutEffect(() => {
        if (focusedObject?.miraType === MiraType.FIELD) {
            const controls = World.sceneRenderer.currentCameraControls
            if (controls instanceof CustomFieldViewControls) {
                setControlsType("FieldView")
            } else {
                World.sceneRenderer.setCameraControls("Target")
                setControlsType("Target")
                const tc = getTargetControls()
                if (tc) tc.focusProvider = focusedObject
            }
            return
        }
        World.sceneRenderer.setCameraControls("Target")
        setControlsType("Target")
        const tc = getTargetControls()
        if (!tc) return
        if (focusedObject) {
            tc.focusProvider = focusedObject
        } else {
            // Explicitly unfocus so validateFocusProvider() does not auto-pick the only loaded object.
            tc.unfocus()
        }
    }, [focusedObject])

    useEffect(() => {
        configureScreen(panel!, { title: "Camera Config", hideAccept: true, cancelText: "Close" }, {})
    }, [configureScreen, panel])

    useEffect(
        () =>
            EventSystem.listen("CameraFocusChangedEvent", ({ focusProvider }) => {
                setFocusedId(focusProvider?.id ?? null)
            }),
        []
    )

    // Refresh object list. If the focused object disappeared, drop focus.
    useEffect(
        () =>
            EventSystem.listen("MirabufObjectChangeEvent", () => {
                const objects = getSceneObjects()
                setSceneObjects(objects)
                setFocusedId(prev => (objects.some(o => o.id === prev) ? prev : null))
            }),
        []
    )

    // Selecting a target only updates state. The layout effect switches controls and assigns focus.
    const onFocusChange = (id: SceneObjectId | null) => setFocusedId(id)

    const targetControls = getTargetControls()

    return (
        <div className="flex flex-col gap-2">
            <div className="flex flex-col gap-1 w-full">
                <span className="text-xs opacity-70 select-none">Focus Target</span>
                <Select
                    value={focusedId}
                    onChange={e => onFocusChange((e.target.value ?? null) as SceneObjectId | null)}
                    size="small"
                    fullWidth
                >
                    <MenuItem value={undefined}>None</MenuItem>
                    {sceneObjects.map(t => (
                        <MenuItem key={t.id} value={t.id}>
                            {t.miraType === MiraType.ROBOT ? t.descriptiveName : t.assemblyName}
                        </MenuItem>
                    ))}
                </Select>
            </div>

            {focusedObject?.miraType === MiraType.ROBOT && controlsType === "Target" && targetControls && (
                <TargetSettings controls={targetControls} />
            )}
            {focusedObject?.miraType === MiraType.FIELD && <FieldViewSettings />}
        </div>
    )
}

export default CameraSelectionPanel
