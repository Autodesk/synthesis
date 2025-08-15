import { Stack, styled } from "@mui/material"
import { Button, ToggleButton, ToggleButtonGroup } from "@/ui/components/StyledComponents"
import { type ChangeEvent, useEffect, useState } from "react"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsTypes"

import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
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
    const { openPanel, closeModal, configureScreen } = useUIContext()

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
                    .then(mirabufSceneObject => {
                        if (mirabufSceneObject) {
                            World.sceneRenderer.registerSceneObject(mirabufSceneObject)

                            if (mirabufSceneObject.miraType == MiraType.ROBOT) {
                                openPanel(InitialConfigPanel, undefined, modal)
                            }
                            closeModal(CloseType.Overwrite)
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
    }, [selectedFile, miraType, openPanel, modal])

    return (
        <Stack className="items-center" gap={5}>
            <ToggleButtonGroup
                value={miraType}
                exclusive
                onChange={(_, v) => v != null && setSelectedType(v)}
                sx={{
                    alignSelf: "center",
                }}
            >
                <ToggleButton value={MiraType.ROBOT}>Robot</ToggleButton>
                <ToggleButton value={MiraType.FIELD}>Field</ToggleButton>
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
