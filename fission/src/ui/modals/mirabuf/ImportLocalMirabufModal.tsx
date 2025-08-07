import { Button, Stack, styled, ToggleButton, ToggleButtonGroup } from "@mui/material"
import { type ChangeEvent, useEffect, useState } from "react"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsTypes"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import World from "@/systems/World"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import type { ConfigurationType } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import InitialConfigPanel from "@/ui/panels/configuring/initial-config/InitialConfigPanel"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"

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

const ImportLocalMirabufModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    const { openPanel, configureScreen } = useUIContext()

    const [selectedFile, setSelectedFile] = useState<File | undefined>(undefined)
    const [miraType, setSelectedType] = useState<MiraType | undefined>(MiraType.ROBOT)

    const onInputChanged = (e: ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const file = e.target.files[0]
            setSelectedFile(file)
        }
    }

    useEffect(() => {
        const onCancel = () => {
            openPanel(ImportMirabufPanel, { configurationType: "ROBOTS" as ConfigurationType })
        }

        const onBeforeAccept = async () => {
            if (selectedFile && miraType != undefined) {
                const hashBuffer = await selectedFile.arrayBuffer()
                World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)
                await MirabufCachingService.cacheAndGetLocalWithInfo(hashBuffer, miraType)
                    .then(x => {
                        if (!x) return undefined

                        return createMirabuf(x.assembly, x.cacheInfo.id, miraType)
                    })
                    .then(x => {
                        if (x) {
                            const { mainSceneObject, gamePieces } = x

                            World.sceneRenderer.registerSceneObject(mainSceneObject)
                            gamePieces?.forEach(async instance => {
                                const assembly = instance.parser.assembly
                                const buffer = mirabuf.Assembly.encode(assembly).finish().buffer as ArrayBuffer

                                const cacheInfo = await MirabufCachingService.cacheLocal(buffer, MiraType.PIECE)
                                if (!cacheInfo) return

                                if (!cacheInfo.name) {
                                    MirabufCachingService.cacheInfo(
                                        cacheInfo.cacheKey,
                                        MiraType.PIECE,
                                        assembly.info?.name ?? undefined
                                    )
                                }

                                const sceneObject = new MirabufSceneObject(instance, assembly.info?.name!, cacheInfo.id)
                                World.sceneRenderer.registerSceneObject(sceneObject)
                            })

                            console.log(`Loaded ${mainSceneObject.miraType.toString()} Locally`)
                            if (mainSceneObject.miraType === MiraType.ROBOT) {
                                globalOpenPanel(InitialConfigPanel, undefined)
                            }
                        }
                    })
                    .finally(() => setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500))
            }
        }

        console.log("HIDE ACCEPT IN IMPL?", selectedFile === undefined || miraType === undefined)

        configureScreen(
            modal!,
            { title: "Import from File", hideAccept: selectedFile === undefined || miraType === undefined },
            { onBeforeAccept, onCancel }
        )
    }, [selectedFile, miraType, openPanel, modal, configureScreen])

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
                <ToggleButton value={MiraType.PIECE}>Piece</ToggleButton>
            </ToggleButtonGroup>
            <Button component="label" role={undefined}>
                Upload File
                <VisuallyHiddenInput type="file" onChange={onInputChanged} multiple accept=".mira" />
            </Button>
            {selectedFile && <Label className="text-center" size="sm">{`Selected File: ${selectedFile.name}`}</Label>}
        </Stack>
    )
}

export default ImportLocalMirabufModal
