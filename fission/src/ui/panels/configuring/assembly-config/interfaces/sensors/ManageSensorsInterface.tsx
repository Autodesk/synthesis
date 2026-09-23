import { Box, Stack } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsTypes"
import type { SensorPreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import { Button, DeleteButton, EditButton, SynthesisIcons } from "@/ui/components/StyledComponents"

const IDENTITY = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]

function persist(sensors: SensorPreferences[], robot: MirabufSceneObject) {
    robot.robotPreferences.sensors = sensors
    robot.savePreferences()
    World.simulationSystem.getSimulationLayer(robot.mechanism)?.refreshSensors()
}

function createNewSensor(count: number): SensorPreferences {
    return {
        name: `Sensor ${count + 1}`,
        sensorType: "gyro",
        device: "SYN AHRS[0]",
        parentNode: undefined,
        deltaTransformation: [...IDENTITY],
    }
}

export type ManageSensorsProps = {
    selectedRobot: MirabufSceneObject
    initialSensors: SensorPreferences[]
    selectSensor: (sensor: SensorPreferences) => void
}

const ManageSensorsInterface: React.FC<ManageSensorsProps> = ({ selectedRobot, initialSensors, selectSensor }) => {
    const [sensors, setSensors] = useState<SensorPreferences[]>(initialSensors)

    const saveEvent = useCallback(() => persist(sensors, selectedRobot), [sensors, selectedRobot])

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", saveEvent)
    }, [saveEvent])

    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [])

    return (
        <Stack gap={2}>
            {sensors.length > 0 ? (
                sensors.map((sensor, i) => (
                    <Box
                        sx={{ bgcolor: "background.paper", p: 2, borderRadius: 5, width: "100%" }}
                        key={`${sensor.name}-${i}`}
                    >
                        <Stack direction="row" gap={2}>
                            <Stack direction="column" gap={1} justifyContent="space-evenly">
                                <Label size="md">{sensor.name}</Label>
                                <Label size="sm">{sensor.sensorType === "gyro" ? "Gyro" : "Accelerometer"}</Label>
                            </Stack>
                            <Stack direction="column" gap={1} justifyContent="space-evenly" ml="auto">
                                <EditButton onClick={() => selectSensor(sensor)} />
                                <DeleteButton
                                    onClick={() => {
                                        const next = sensors.filter((_, idx) => idx !== i)
                                        setSensors(next)
                                        persist(next, selectedRobot)
                                    }}
                                />
                            </Stack>
                        </Stack>
                    </Box>
                ))
            ) : (
                <Label size="md">No sensors</Label>
            )}
            <Button
                color="success"
                variant="contained"
                className="w-full"
                onClick={() => {
                    const sensor = createNewSensor(sensors.length)
                    const next = [...sensors, sensor]
                    setSensors(next)
                    persist(next, selectedRobot)
                    selectSensor(sensor)
                }}
            >
                <SynthesisIcons.ADD_LARGE />
            </Button>
        </Stack>
    )
}

export default ManageSensorsInterface
