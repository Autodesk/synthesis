import { Button, Stack, styled, ToggleButton, ToggleButtonGroup, Typography } from "@mui/material"
import { type ChangeEvent, useContext, useEffect, useRef, useState } from "react"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsSystem"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import World from "@/systems/World"
import type { ModalImplProps } from "@/ui/components/Modal"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import { UIContext } from "@/ui/UIProvider"
import InitialConfigPanel from "@/ui/panels/configuring/initial-config/InitialConfigPanel"

const VisuallyHiddenInput = styled("input")({
    clip: "rect(0 0 0 0)",
    clipPath: "inset(50%)",
    height: 1,
    overflow: "hidden",
    position: "absolute",
    bottom: 0,
    left: 0,
    whiteSpace: "nowrap",
    width: 1,
})

const ImportLocalMirabufModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    const { openPanel } = useContext(UIContext)

    const [selectedFile, setSelectedFile] = useState<File | undefined>(undefined)
    const [miraType, setSelectedType] = useState<MiraType | undefined>(MiraType.ROBOT)

    const onInputChanged = (e: ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const file = e.target.files[0]
            setSelectedFile(file)
        }
    }

    useEffect(() => {
        modal!.props.onCancel = () => openPanel(<ImportMirabufPanel />, undefined)
        modal!.props.onAccept = async () => {
            if (selectedFile && miraType !== undefined) {
                const hashBuffer = await selectedFile.arrayBuffer()
                World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)
                await MirabufCachingService.cacheAndGetLocalWithInfo(hashBuffer, miraType)
                    .then(result => {
                        if (result) {
                            return createMirabuf(result.assembly, undefined, result.cacheInfo.id)
                        }
                        return undefined
                    })
                    .then(x => {
                        if (x) {
                            World.sceneRenderer.registerSceneObject(x)

                            openPanel(<InitialConfigPanel />, modal)
                        }
                    })
                    .finally(() => setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500))
            }
        }
    }, [selectedFile])

    return (
        <Stack className="items-center" gap={5}>
            <ToggleButtonGroup
                value={miraType}
                exclusive
                onChange={(_, v) => v != null && setSelectedType(v)}
                {...SoundPlayer.buttonSoundEffects()}
                sx={{
                    alignSelf: "center",
                }}
            >
                <ToggleButton value={MiraType.ROBOT}>Robot</ToggleButton>
                <ToggleButton value={MiraType.FIELD}>Field</ToggleButton>
            </ToggleButtonGroup>
            <Button component="label" role={undefined}>
                Upload File
                <VisuallyHiddenInput type="file" onChange={onInputChanged} multiple />
            </Button>
            {selectedFile && (
                <Typography className="text-center" variant="h5">{`Selected File: ${selectedFile.name}`}</Typography>
            )}
        </Stack>
    )
}

export default ImportLocalMirabufModal
