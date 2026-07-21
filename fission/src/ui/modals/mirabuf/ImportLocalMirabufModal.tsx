import { Stack, styled } from "@mui/material"
import { type ChangeEvent, useEffect, useState } from "react"
import { globalOpenModal } from "@/components/GlobalUIControls.ts"
import MirabufCachingService, { getGamePieceTypeName, MiraType } from "@/mirabuf/MirabufLoader"
import { zeroGamePieceInstancePosition } from "@/mirabuf/MirabufParser"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsTypes"
import World from "@/systems/World"
import { loadURDF } from "@/urdf/URDFLoader"
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
import { getTargetControls } from "@/systems/scene/CameraControls"
import { hashBuffer } from "@/util/Utility.ts"

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
    errorMessage?: string
}

function isURDFFile(filename: string): boolean {
    return filename.split(".").pop()?.toLowerCase() === "zip"
}

const ImportLocalMirabufModal: React.FC<ModalImplProps<void, ImportLocalMirabufProps>> = ({ modal }) => {
    const { openPanel, closeModal, configureScreen } = useUIContext()

    const { configurationType, errorMessage } = modal!.props.custom

    const [selectedFile, setSelectedFile] = useState<File | undefined>(undefined)
    const [miraType, setSelectedType] = useState<MiraType | undefined>()
    const [isUrdf, setIsUrdf] = useState(false)
    const [importError, setImportError] = useState<string | undefined>(errorMessage)

    const onInputChanged = (e: ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const file = e.target.files[0]
            const ext = file.name.split(".").pop()?.toLowerCase()
            if (ext === "urdf") {
                setImportError(
                    "Plain URDF files are not supported. Please select a ZIP archive containing the URDF and its meshes."
                )
                setSelectedFile(undefined)
                setIsUrdf(false)
                return
            }
            setImportError(undefined)
            setSelectedFile(file)
            if (isURDFFile(file.name)) {
                setIsUrdf(true)
                setSelectedType(MiraType.ROBOT)
            } else {
                setIsUrdf(false)
            }
        }
    }

    useEffect(() => {
        const onCancel = () => {
            openPanel(ImportMirabufPanel, { configurationType: miraTypeToConfigType(miraType ?? MiraType.ROBOT) })
        }

        const onBeforeAccept = async () => {
            if (!selectedFile || miraType === undefined) return

            const buffer = await selectedFile.arrayBuffer()
            World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)

            try {
                let result: Awaited<ReturnType<typeof createMirabuf>>

                if (isURDFFile(selectedFile.name)) {
                    const assembly = await loadURDF(buffer, selectedFile.name)
                    const hash = await hashBuffer(buffer)
                    result = await createMirabuf(hash, assembly, hash, miraType)
                } else {
                    const cached = await MirabufCachingService.cacheLocalAndReturn(buffer, miraType)
                    if (!cached) {
                        globalOpenModal(ImportLocalMirabufModal, {
                            configurationType: miraTypeToConfigType(miraType),
                        })
                        return
                    }
                    result = await createMirabuf(
                        cached.cacheInfo.hash,
                        cached.assembly,
                        cached.cacheInfo.hash,
                        miraType
                    )
                }

                if (!result) {
                    globalOpenModal(ImportLocalMirabufModal, {
                        configurationType: miraTypeToConfigType(miraType),
                    })
                    return
                }

                const { mainSceneObject, gamePieces } = result
                World.sceneRenderer.registerSceneObject(mainSceneObject)

                // Only one cache entry is kept per game piece type (e.g. "Cube"), so instances
                // sharing a type reuse the existing cached entry instead of accumulating duplicates.
                const cachedTypeNames = new Set(MirabufCachingService.getAll(MiraType.PIECE).map(i => i.name))
                for (const instance of gamePieces ?? []) {
                    const pieceAssembly = instance.parser.assembly
                    const typeName = getGamePieceTypeName(pieceAssembly?.info?.name ?? "Piece")

                    if (cachedTypeNames.has(typeName)) {
                        const existing = MirabufCachingService.getAll(MiraType.PIECE).find(i => i.name === typeName)
                        const sceneObject = new MirabufSceneObject(instance, existing?.hash ?? "")
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

                    const sceneObject = new MirabufSceneObject(instance, cacheInfo.hash)
                    World.sceneRenderer.registerSceneObject(sceneObject)
                }

                if (mainSceneObject.miraType == MiraType.ROBOT || mainSceneObject.miraType == MiraType.PIECE) {
                    openPanel(InitialConfigPanel, undefined, modal)
                }
                const targetControls = getTargetControls()
                if (targetControls && (miraType === MiraType.ROBOT || !targetControls.focusProvider)) {
                    targetControls.focusProvider = mainSceneObject
                }
                closeModal(CloseType.Overwrite)
            } catch (e) {
                console.error("[Import]", e)
                globalOpenModal(ImportLocalMirabufModal, {
                    configurationType: miraTypeToConfigType(miraType),
                    errorMessage: e instanceof Error ? e.message : "An unknown error occurred during import.",
                })
            } finally {
                setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500)
            }
        }

        configureScreen(
            modal!,
            { title: "Import from File", hideAccept: selectedFile === undefined || miraType === undefined },
            { onBeforeAccept, onCancel }
        )
    }, [selectedFile, miraType, isUrdf, openPanel, modal, closeModal, configureScreen])

    useEffect(() => {
        setSelectedType(configTypeToMiraType(configurationType))
    }, [configurationType])

    return (
        <Stack className="items-center" gap={5}>
            {!isUrdf && (
                <ToggleButtonGroup
                    value={miraType}
                    exclusive
                    onChange={(_, v) => v != null && setSelectedType(v)}
                    sx={{ alignSelf: "center" }}
                >
                    <ToggleButton value={MiraType.ROBOT}>Robot</ToggleButton>
                    <ToggleButton value={MiraType.FIELD}>Field</ToggleButton>
                    <ToggleButton value={MiraType.PIECE}>Piece</ToggleButton>
                </ToggleButtonGroup>
            )}
            <Button component="label" role={undefined}>
                Upload File
                <VisuallyHiddenInput type="file" onChange={onInputChanged} multiple accept=".mira,.urdf,.zip" />
            </Button>
            {importError && (
                <Label className="text-center" size="sm" style={{ color: "red" }}>
                    {importError}
                </Label>
            )}
            {selectedFile && <Label className="text-center" size="sm">{`Selected File: ${selectedFile.name}`}</Label>}
        </Stack>
    )
}

export default ImportLocalMirabufModal
