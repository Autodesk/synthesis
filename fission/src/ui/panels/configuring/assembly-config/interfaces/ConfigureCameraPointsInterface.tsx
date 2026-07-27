import { Divider, MenuItem, Select, Stack, TextField } from "@mui/material"
import { useCallback, useEffect, useRef, useState } from "react"
import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { CameraLook, CameraPoint } from "@/systems/preferences/PreferenceTypes"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import ScrollView from "@/ui/components/ScrollView"
import { AddButton, DeleteButton, EditButton } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"

const RAD_TO_DEG = 180 / Math.PI
const DEG_TO_RAD = Math.PI / 180

/** Field-relative point with mutable fields for editing. */
type EditablePoint = {
    name: string
    pos: [number, number, number]
    look: CameraLook
}

function toEditable(p: CameraPoint): EditablePoint {
    return { name: p.name, pos: [p.pos[0], p.pos[1], p.pos[2]], look: { ...p.look } as CameraLook }
}

function persist(points: CameraPoint[], field: MirabufSceneObject) {
    if (!field.fieldPreferences) return
    field.fieldPreferences.cameraPoints = points
    PreferencesSystem.savePreferences()
}

interface ListViewProps {
    selectedField: MirabufSceneObject
    points: CameraPoint[]
    onChange: (points: CameraPoint[]) => void
    onAdd: () => void
    onEdit: (index: number) => void
}

const ListView: React.FC<ListViewProps> = ({ selectedField, points, onChange, onAdd, onEdit }) => {
    const saveEvent = useCallback(() => persist(points, selectedField), [points, selectedField])

    useEffect(() => EventSystem.listen("ConfigurationSavedEvent", saveEvent), [saveEvent])
    useEffect(() => {
        persist(points, selectedField)
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [selectedField, points])

    return (
        <>
            {points.length > 0 ? (
                <ScrollView>
                    <Stack gap={4}>
                        {points.map((p, i) => (
                            <Stack
                                direction="row"
                                // TODO:
                                // biome-ignore lint/suspicious/noArrayIndexKey: currently not a better way to do this.
                                key={`${p.name}-${i}`}
                                justifyContent="space-between"
                                alignItems="center"
                                gap="1rem"
                            >
                                <Label size="sm">{p.name}</Label>
                                <Stack direction="row-reverse" gap="0.25rem" alignItems="center">
                                    <EditButton
                                        onClick={() => {
                                            saveEvent()
                                            onEdit(i)
                                        }}
                                    />
                                    <DeleteButton
                                        onClick={() => {
                                            const next = points.filter((_, idx) => idx !== i)
                                            onChange(next)
                                            persist(next, selectedField)
                                        }}
                                    />
                                </Stack>
                            </Stack>
                        ))}
                    </Stack>
                </ScrollView>
            ) : (
                <Label size="sm">No camera positions</Label>
            )}
            <AddButton onClick={onAdd} />
        </>
    )
}

interface EditViewProps {
    selectedField: MirabufSceneObject
    point: EditablePoint
    onSave: (updated: CameraPoint) => void
}

const EditView: React.FC<EditViewProps> = ({ selectedField, point, onSave }) => {
    const [name, setName] = useState(point.name)
    const [lookType, setLookType] = useState<CameraLook["type"]>(point.look.type)
    const [yawDeg, setYawDeg] = useState(point.look.type === "rotation" ? Math.round(point.look.yaw * RAD_TO_DEG) : 0)
    const [pitchDeg, setPitchDeg] = useState(
        point.look.type === "rotation" ? Math.round(point.look.pitch * RAD_TO_DEG) : -30
    )
    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const postGizmoCreation = useCallback(
        (gizmo: GizmoSceneObject) => {
            const fieldRef = selectedField.getXZPositionTransform()
            gizmo.obj.position.set(fieldRef.x + point.pos[0], fieldRef.y + point.pos[1], fieldRef.z + point.pos[2])
        },
        [selectedField, point.pos]
    )

    const buildPoint = useCallback((): CameraPoint => {
        let pos: [number, number, number] = [point.pos[0], point.pos[1], point.pos[2]]
        if (gizmoRef.current) {
            gizmoRef.current.obj.updateWorldMatrix(true, false)
            const worldPos = gizmoRef.current.obj.getWorldPosition(new THREE.Vector3())
            const fieldRef = selectedField.getXZPositionTransform()
            pos = [worldPos.x - fieldRef.x, worldPos.y - fieldRef.y, worldPos.z - fieldRef.z]
        }
        const look: CameraLook =
            lookType === "field"
                ? { type: "field" }
                : { type: "rotation", yaw: yawDeg * DEG_TO_RAD, pitch: pitchDeg * DEG_TO_RAD }
        return { name, pos, look }
    }, [selectedField, point.pos, lookType, name, yawDeg, pitchDeg])

    useEffect(() => EventSystem.listen("ConfigurationSavedEvent", () => onSave(buildPoint())), [buildPoint, onSave])
    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [])

    return (
        <Stack gap={2} className="bg-background-secondary rounded-md p-2">
            <TextField
                label="Name"
                placeholder="Camera position name"
                defaultValue={name}
                onChange={e => setName(e.target.value)}
            />

            <Stack gap={1}>
                <span className="text-xs opacity-70">Look Direction</span>
                <Select
                    value={lookType}
                    onChange={e => setLookType(e.target.value as CameraLook["type"])}
                    size="small"
                    fullWidth
                >
                    <MenuItem value="field">Field Center</MenuItem>
                    <MenuItem value="rotation">Fixed Rotation</MenuItem>
                </Select>
            </Stack>

            {lookType === "rotation" && (
                <Stack gap={2}>
                    <TextField
                        label="Yaw (degrees)"
                        type="number"
                        value={yawDeg}
                        onChange={e => setYawDeg(Number(e.target.value) || 0)}
                        size="small"
                    />
                    <TextField
                        label="Pitch (degrees)"
                        type="number"
                        value={pitchDeg}
                        onChange={e => setPitchDeg(Number(e.target.value) || 0)}
                        size="small"
                        helperText="-90° looks straight down"
                    />
                </Stack>
            )}

            <TransformGizmoControl
                key="camera-point-gizmo"
                size={1.5}
                gizmoRef={gizmoRef}
                defaultMode="translate"
                rotateDisabled={true}
                scaleDisabled={true}
                postGizmoCreation={postGizmoCreation}
            />
        </Stack>
    )
}

interface ConfigureCameraPointsProps {
    selectedField: MirabufSceneObject
    initialPoints: CameraPoint[]
}

const ConfigureCameraPointsInterface: React.FC<ConfigureCameraPointsProps> = ({ selectedField, initialPoints }) => {
    const [points, setPoints] = useState<CameraPoint[]>(initialPoints)
    const [editIndex, setEditIndex] = useState<number | undefined>(undefined)

    const updatePoint = useCallback(
        (idx: number, updated: CameraPoint) => {
            setPoints(prev => {
                const next = [...prev]
                next[idx] = updated
                persist(next, selectedField)
                return next
            })
        },
        [selectedField]
    )

    const handleAdd = () => {
        const newPoint: CameraPoint = { name: "New Camera", pos: [0, 3, 0], look: { type: "field" } }
        setPoints(prev => {
            const next = [...prev, newPoint]
            persist(next, selectedField)
            setEditIndex(next.length - 1)
            return next
        })
    }

    if (editIndex !== undefined && points[editIndex] !== undefined) {
        return (
            <>
                <Stack direction="row" minHeight="30px" alignItems="center">
                    <Label size="sm" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                        Configuring Camera Position
                    </Label>
                </Stack>
                <Divider />
                <EditView
                    selectedField={selectedField}
                    point={toEditable(points[editIndex])}
                    onSave={updated => updatePoint(editIndex, updated)}
                />
            </>
        )
    }

    return (
        <ListView
            selectedField={selectedField}
            points={points}
            onChange={newPoints => {
                setPoints(newPoints)
                persist(newPoints, selectedField)
            }}
            onAdd={handleAdd}
            onEdit={idx => setEditIndex(idx)}
        />
    )
}

export default ConfigureCameraPointsInterface
