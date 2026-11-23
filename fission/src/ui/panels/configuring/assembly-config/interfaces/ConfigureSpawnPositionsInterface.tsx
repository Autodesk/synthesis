import { Box, Stack, TextField } from "@mui/material"
import { useCallback, useEffect, useMemo, useState } from "react"
import { SelectMenuHeader } from "@/components/SelectMenu.tsx"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import {
    ALLIANCES,
    type Alliance,
    defaultFieldPreferences,
    type FieldPreferences,
    type SpawnLocation,
    STATIONS,
    type Station,
} from "@/systems/preferences/PreferenceTypes"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import Label from "@/ui/components/Label"
import ScrollView from "@/ui/components/ScrollView"
import { EditButton } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { useConfigurationSavedListener, useHoldPhysicsPause } from "@/util/ReactHooks.ts"
import {
    useDirectionIndicatorMesh,
    useFieldPointMarkers,
    useFieldRelativeGizmoPosition,
    useSyncIndicatorRotation,
} from "./FieldPointEditing"
import { capitalize } from "@/util/Utility"

const RAD_TO_DEG = 180 / Math.PI
const DEG_TO_RAD = Math.PI / 180

type SpawnLocations = FieldPreferences["spawnLocations"]

/** Where one spawn slot lives within {@link SpawnLocations}: the default location, or one alliance/station pair. */
type SpawnSlotPath = "default" | { alliance: Alliance; station: Station }

/** A fixed, named place within {@link SpawnLocations}. */
interface SpawnSlot {
    id: string
    label: string
    path: SpawnSlotPath
    alliance?: Alliance
}

function getSpawnLocation(locations: SpawnLocations, path: SpawnSlotPath): SpawnLocation {
    return path === "default" ? locations.default : locations[path.alliance][path.station]
}

function setSpawnLocation(locations: SpawnLocations, path: SpawnSlotPath, value: SpawnLocation): SpawnLocations {
    if (path === "default") return { ...locations, default: value }
    return { ...locations, [path.alliance]: { ...locations[path.alliance], [path.station]: value } }
}

const ALLIANCE_COLORS: Record<Alliance, string> = {
    red: "redAlliance.main",
    blue: "blueAlliance.main",
}

function allianceColor(alliance: Alliance | undefined): string {
    return alliance ? ALLIANCE_COLORS[alliance] : "grey.500"
}

/** Every configurable spawn slot: the default location, then each alliance's stations. */
const SPAWN_SLOTS: SpawnSlot[] = [
    { id: "default", label: "Default", path: "default" },
    ...ALLIANCES.flatMap(alliance =>
        STATIONS.map(
            (station): SpawnSlot => ({
                id: `${alliance}-${station}`,
                label: `${capitalize(alliance)} Station ${station}`,
                path: { alliance, station },
                alliance,
            })
        )
    ),
]

function persist(locations: SpawnLocations, field: MirabufSceneObject) {
    if (!field.fieldPreferences) return
    field.fieldPreferences.spawnLocations = locations
    PreferencesSystem.savePreferences()
}

interface ListViewProps {
    selectedField: MirabufSceneObject
    locations: SpawnLocations
    onEdit: (slot: SpawnSlot) => void
}

const ListView: React.FC<ListViewProps> = ({ selectedField, locations, onEdit }) => {
    const saveEvent = useCallback(() => persist(locations, selectedField), [locations, selectedField])

    useConfigurationSavedListener(saveEvent)
    useHoldPhysicsPause()

    const markerPoints = useMemo(() => SPAWN_SLOTS.map(slot => getSpawnLocation(locations, slot.path)), [locations])
    useFieldPointMarkers(selectedField, markerPoints)

    return (
        <ScrollView>
            <Stack gap={2}>
                {SPAWN_SLOTS.map(slot => (
                    <Box sx={{ bgcolor: "background.paper", p: 2, borderRadius: 5, width: "100%" }} key={slot.id}>
                        <Stack direction="row" gap={2}>
                            <Box className="w-12 rounded-lg" sx={{ bgcolor: allianceColor(slot.alliance) }} />
                            <Stack direction="column" justifyContent="space-evenly">
                                <Label size="md">{slot.label}</Label>
                            </Stack>
                            <Stack direction="column" justifyContent="space-evenly" ml="auto">
                                <EditButton
                                    onClick={() => {
                                        saveEvent()
                                        onEdit(slot)
                                    }}
                                />
                            </Stack>
                        </Stack>
                    </Box>
                ))}
            </Stack>
        </ScrollView>
    )
}

interface EditViewProps {
    selectedField: MirabufSceneObject
    location: SpawnLocation
    onSave: (updated: SpawnLocation) => void
}

const EditView: React.FC<EditViewProps> = ({ selectedField, location, onSave }) => {
    const [yawDeg, setYawDeg] = useState(Math.round(location.yaw * RAD_TO_DEG))
    const { gizmoRef, postGizmoCreation, readFieldRelativePosition } = useFieldRelativeGizmoPosition(
        selectedField,
        location.pos
    )
    const directionIndicatorMesh = useDirectionIndicatorMesh()
    useSyncIndicatorRotation(directionIndicatorMesh, yawDeg * DEG_TO_RAD)

    const setupGizmo = useCallback(
        (gizmo: GizmoSceneObject) => {
            postGizmoCreation(gizmo)
            gizmo.obj.add(directionIndicatorMesh)
        },
        [postGizmoCreation, directionIndicatorMesh]
    )

    const buildLocation = useCallback(
        (): SpawnLocation => ({ pos: readFieldRelativePosition(), yaw: yawDeg * DEG_TO_RAD }),
        [readFieldRelativePosition, yawDeg]
    )

    useConfigurationSavedListener(useCallback(() => onSave(buildLocation()), [buildLocation, onSave]))
    useHoldPhysicsPause()

    return (
        <Stack gap={2} className="bg-background-secondary rounded-md p-2">
            <TextField
                label="Yaw (degrees)"
                type="number"
                value={yawDeg}
                onChange={e => setYawDeg(Number(e.target.value) || 0)}
                size="small"
            />

            <TransformGizmoControl
                key="spawn-location-gizmo"
                size={1.5}
                gizmoRef={gizmoRef}
                defaultMode="translate"
                rotateDisabled={true}
                scaleDisabled={true}
                postGizmoCreation={setupGizmo}
            />
        </Stack>
    )
}

const ConfigureSpawnPositionsInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    registerCleanupFunction,
}) => {
    const [locations, setLocations] = useState<SpawnLocations>(
        selectedAssembly.fieldPreferences?.spawnLocations ?? defaultFieldPreferences().spawnLocations
    )
    const [editSlot, setEditSlot] = useState<SpawnSlot | undefined>(undefined)

    useEffect(() => {
        const initial = structuredClone(selectedAssembly.fieldPreferences!.spawnLocations)
        registerCleanupFunction(undefined, () => {
            const prefs = selectedAssembly.fieldPreferences
            if (prefs == null) return
            prefs.spawnLocations = initial
        })
    }, [registerCleanupFunction, selectedAssembly])

    const updateLocation = useCallback(
        (slot: SpawnSlot, updated: SpawnLocation) => {
            const next: SpawnLocations = {
                ...setSpawnLocation(locations, slot.path, updated),
                hasConfiguredLocations: true,
            }
            persist(next, selectedAssembly)
            setLocations(next)
        },
        [locations, selectedAssembly]
    )

    if (editSlot !== undefined) {
        return (
            <>
                <SelectMenuHeader
                    label={`${editSlot.label} Spawn Position`}
                    showBackButton={true}
                    onBackButton={() => {
                        EventSystem.dispatch("ConfigurationSavedEvent")
                        setEditSlot(undefined)
                    }}
                />
                <EditView
                    selectedField={selectedAssembly}
                    location={getSpawnLocation(locations, editSlot.path)}
                    onSave={updated => updateLocation(editSlot, updated)}
                />
            </>
        )
    }

    return <ListView selectedField={selectedAssembly} locations={locations} onEdit={setEditSlot} />
}

export default ConfigureSpawnPositionsInterface
