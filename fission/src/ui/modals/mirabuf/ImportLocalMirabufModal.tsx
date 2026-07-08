import { Stack, styled } from "@mui/material"
import { type ChangeEvent, useEffect, useState } from "react"
import { globalOpenModal } from "@/components/GlobalUIControls.ts"
import MirabufCachingService, { getGamePieceTypeName, MiraType } from "@/mirabuf/MirabufLoader"
import { zeroGamePieceInstancePosition } from "@/mirabuf/MirabufParser"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsTypes"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import { Button, ToggleButton, ToggleButtonGroup } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import {
    type ConfigurationType,
    configTypeToMiraType,
    miraTypeToConfigType,
} from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import InitialConfigPanel from "@/ui/panels/configuring/initial-config/InitialConfigPanel"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import type { CustomTargetControls } from "@/systems/scene/CameraControls"

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
interface ImportLocalMirabufProps {
    configurationType: ConfigurationType
}

const ImportLocalMirabufModal: React.FC<ModalImplProps<void, ImportLocalMirabufProps>> = ({ modal }) => {
    // update tooltip based on type of drivetrain, receive message from Synthesis
    const { openPanel, closeModal, configureScreen } = useUIContext()

    const { configurationType } = modal!.props.custom

    const [selectedFile, setSelectedFile] = useState<File | undefined>(undefined)
    const [miraType, setSelectedType] = useState<MiraType | undefined>()

    const onInputChanged = (e: ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const file = e.target.files[0]
            setSelectedFile(file)
        }
    }

    useEffect(() => {
        const onCancel = () => {
            openPanel(ImportMirabufPanel, { configurationType: miraTypeToConfigType(miraType ?? MiraType.ROBOT) })
        }

        const onBeforeAccept = async () => {
            if (selectedFile && miraType !== undefined) {
                const buffer = await selectedFile.arrayBuffer()
                World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)
                await MirabufCachingService.cacheLocalAndReturn(buffer, miraType)
                    .then(result => {
                        if (result) {
                            return createMirabuf(result.assembly, result.cacheInfo.hash, miraType, undefined)
                        }
                        globalOpenModal(ImportLocalMirabufModal, {
                            configurationType: miraTypeToConfigType(miraType ?? MiraType.ROBOT),
                        })
                        return undefined
                    })
                    .then(async mirabufSceneObject => {
                        if (mirabufSceneObject) {
                            const { mainSceneObject, gamePieces } = mirabufSceneObject

                            World.sceneRenderer.registerSceneObject(mainSceneObject)

                            // Only one cache entry is kept per game piece type (e.g. "Cube"), so instances
                            // sharing a type reuse the existing cached entry instead of accumulating duplicates.
                            const cachedTypeNames = new Set(
                                MirabufCachingService.getAll(MiraType.PIECE).map(i => i.name)
                            )
                            for (const instance of gamePieces ?? []) {
                                const pieceAssembly = instance.parser.assembly
                                const typeName = getGamePieceTypeName(pieceAssembly?.info?.name ?? "Piece")

                                if (cachedTypeNames.has(typeName)) {
                                    const existing = MirabufCachingService.getAll(MiraType.PIECE).find(
                                        i => i.name === typeName
                                    )
                                    const sceneObject = new MirabufSceneObject(instance, typeName, existing?.hash ?? "")
                                    World.sceneRenderer.registerSceneObject(sceneObject)
                                    continue
                                }
                                cachedTypeNames.add(typeName)

                                zeroGamePieceInstancePosition(pieceAssembly)
                                const cacheInfo = await MirabufCachingService.storeAssemblyInCache(pieceAssembly, {
                                    miraType: MiraType.PIECE,
                                    name: typeName,
                                })
                                if (!cacheInfo) continue

                                const sceneObject = new MirabufSceneObject(instance, typeName, cacheInfo.hash)
                                World.sceneRenderer.registerSceneObject(sceneObject)
                            }

                            if (
                                mainSceneObject.miraType == MiraType.ROBOT ||
                                mainSceneObject.miraType == MiraType.PIECE
                            ) {
                                openPanel(InitialConfigPanel, undefined, modal)
                            }
                            const cameraControls = World.sceneRenderer.currentCameraControls as CustomTargetControls
                            if (miraType === MiraType.ROBOT || !cameraControls.focusProvider) {
                                cameraControls.focusProvider = mainSceneObject
                            }
                            closeModal(CloseType.Overwrite)
                        }
                    })
                    .finally(() => setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500))
            }
        }

        configureScreen(
            modal!,
            { title: "Import from File", hideAccept: selectedFile === undefined || miraType === undefined },
            { onBeforeAccept, onCancel }
        )
    }, [selectedFile, miraType, openPanel, modal, closeModal, configureScreen])

    useEffect(() => {
        setSelectedType(configTypeToMiraType(configurationType))
    }, [configurationType])
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
