import { Box, Divider, Stack } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsTypes"
import { type CameraPreferences, defaultCameraPreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import { Button, DeleteButton, EditButton, SynthesisIcons } from "@/ui/components/StyledComponents"
import CameraConfigInterface from "./CameraConfigInterface"
import Label from "@/ui/components/Label"
import { SelectMenuHeader } from "@/ui/components/SelectMenu"
import EventSystem from "@/systems/EventSystem"
import type { ConfigurationSubpanelComponent } from "../../ConfigTypes"

const ConfigureCameraInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    setDisableAccept,
    registerCleanupFunction,
}) => {
    const [_version, setVersion] = useState(0)
    const [selectedCamera, setSelectedCamera] = useState<CameraPreferences | null>(null)

    const cameras = selectedAssembly.cameraPreferences

    const forceRender = useCallback(() => setVersion(v => v + 1), [])

    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [])

    useEffect(() => {
        const originalCameras = structuredClone(selectedAssembly.cameraPreferences)
        registerCleanupFunction(undefined, () => {
            selectedAssembly.cameraPreferences = originalCameras
            selectedAssembly.updateCameras()
        })
    }, [registerCleanupFunction, selectedAssembly])

    return (
        <>
            {selectedCamera !== null ? (
                <>
                    <SelectMenuHeader
                        label={`Camera ${selectedCamera.name}`}
                        showBackButton={true}
                        onBackButton={() => {
                            EventSystem.dispatch("ConfigurationSavedEvent")
                            setSelectedCamera(null)
                        }}
                    />
                    <Divider />
                    <CameraConfigInterface camera={selectedCamera} selectedRobot={selectedAssembly} setDisableAccept={setDisableAccept} />
                </>
            ) : (
                <Stack gap={2}>
                    {cameras.length > 0 ? (
                        cameras.map(cameraPrefs => (
                            <Box
                                sx={{ bgcolor: "background.paper", p: 2, borderRadius: 5, width: "100%" }}
                                key={`${cameraPrefs.id}`}
                            >
                                <Stack direction="row" gap={2} justifyContent="space-between">
                                    <Label size="md">{cameraPrefs.name}</Label>
                                    <Stack direction="row" gap={1} justifyContent="space-evenly" ml="auto">
                                        <EditButton
                                            onClick={() => {
                                                setSelectedCamera(cameraPrefs)
                                            }}
                                        />
                                        <DeleteButton
                                            onClick={() => {
                                                selectedAssembly.cameraPreferences =
                                                    selectedAssembly.cameraPreferences.filter(
                                                        cpref => cpref.id !== cameraPrefs.id
                                                    )
                                                setSelectedCamera(null)
                                                selectedAssembly.updateCameras()
                                                forceRender()
                                            }}
                                        />
                                    </Stack>
                                </Stack>
                            </Box>
                        ))
                    ) : (
                        <Label size="md">No cameras configured. Add one to get started.</Label>
                    )}
                    <Button
                        color="success"
                        variant="contained"
                        onClick={() => {
                            const nextId = cameras.reduce((max, c) => Math.max(max, c.id + 1), 0)
                            cameras.push(defaultCameraPreferences(nextId))
                            setSelectedCamera(cameras.at(-1)!) // should be safe since we just added one
                            selectedAssembly.updateCameras()
                            forceRender()
                        }}
                        className="w-full"
                    >
                        <SynthesisIcons.ADD_LARGE />
                    </Button>
                </Stack>
            )}
        </>
    )
}

export default ConfigureCameraInterface
