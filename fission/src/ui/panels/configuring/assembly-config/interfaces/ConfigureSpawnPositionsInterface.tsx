import { Box, Stack, TextField } from "@mui/material"
import { useCallback, useState } from "react"
import { SelectMenuHeader } from "@/components/SelectMenu.tsx"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import {
    ALLIANCES,
    type Alliance,
    type FieldPreferences,
    type SpawnLocation,
    STATIONS,
    type Station,
} from "@/systems/preferences/PreferenceTypes"
import Label from "@/ui/components/Label"
import ScrollView from "@/ui/components/ScrollView"
import { EditButton } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import {
    useConfigurationSavedListener,
    useFieldRelativeGizmoPosition,
    useHoldPhysicsPauseWhileMounted,
} from "./FieldPointEditing"

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

function capitalize(word: string): string {
    return word[0].toUpperCase() + word.slice(1)
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
    useHoldPhysicsPauseWhileMounted()

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

    const buildLocation = useCallback(
        (): SpawnLocation => ({ pos: readFieldRelativePosition(), yaw: yawDeg * DEG_TO_RAD }),
        [readFieldRelativePosition, yawDeg]
    )

    useConfigurationSavedListener(useCallback(() => onSave(buildLocation()), [buildLocation, onSave]))
    useHoldPhysicsPauseWhileMounted()

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
                postGizmoCreation={postGizmoCreation}
            />
        </Stack>
    )
}

interface ConfigureSpawnPositionsProps {
    selectedField: MirabufSceneObject
    initialLocations: SpawnLocations
}

const ConfigureSpawnPositionsInterface: React.FC<ConfigureSpawnPositionsProps> = ({
    selectedField,
    initialLocations,
}) => {
    const [locations, setLocations] = useState<SpawnLocations>(initialLocations)
    const [editSlot, setEditSlot] = useState<SpawnSlot | undefined>(undefined)

    const updateLocation = useCallback(
        (slot: SpawnSlot, updated: SpawnLocation) => {
            setLocations(prev => {
                const next = setSpawnLocation(prev, slot.path, updated)
                next.hasConfiguredLocations = true
                persist(next, selectedField)
                return next
            })
        },
        [selectedField]
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
                    selectedField={selectedField}
                    location={getSpawnLocation(locations, editSlot.path)}
                    onSave={updated => updateLocation(editSlot, updated)}
                />
            </>
        )
    }

    return <ListView selectedField={selectedField} locations={locations} onEdit={setEditSlot} />
}

export default ConfigureSpawnPositionsInterface
