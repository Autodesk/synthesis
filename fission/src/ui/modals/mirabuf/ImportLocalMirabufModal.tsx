import { Stack, styled } from "@mui/material"
import { type ChangeEvent, useCallback, useEffect, useState } from "react"
import { globalOpenModal } from "@/components/GlobalUIControls.ts"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsTypes"
import World from "@/systems/World"
import { loadURDF } from "@/urdf/URDFLoader"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import { Button, ToggleButton, ToggleButtonGroup } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import {
    configTypeToMiraType,
    type ConfigurationType,
    miraTypeToConfigType,
} from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import InitialConfigPanel from "@/ui/panels/configuring/initial-config/InitialConfigPanel"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import { getTargetControls } from "@/systems/scene/CameraControls"
import { hashBuffer, hexStringToUint8Array } from "@/util/Utility.ts"
import { ProgressHandle, URDFImportProgressBar } from "@/components/ProgressNotificationData.ts"
import { v4 as uuidV4 } from "uuid"
import WheelAssignmentPanel from "@/components/WheelAssignmentPanel.tsx"

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
    const { openPanel, closeModal, configureScreen, openModal } = useUIContext()

    const { configurationType, errorMessage } = modal!.props.custom

    const [selectedFile, setSelectedFile] = useState<File | undefined>(undefined)
    const [miraType, setSelectedType] = useState<MiraType | undefined>()
    const [isUrdf, setIsUrdf] = useState(false)
    const [importError, setImportError] = useState<string | undefined>(errorMessage)

    const onInputChanged = useCallback((e: ChangeEvent<HTMLInputElement>) => {
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
    }, [])
    const onCancel = useCallback(() => {
        openPanel(ImportMirabufPanel, { configurationType: miraTypeToConfigType(miraType ?? MiraType.ROBOT) })
    }, [openPanel, miraType])

    const finalizeSceneObject = useCallback(
        (mirabufSceneObject: MirabufSceneObject) => {
            World.sceneRenderer.registerSceneObject(mirabufSceneObject)
            const targetControls = getTargetControls()
            if (targetControls && (miraType === MiraType.ROBOT || !targetControls.focusProvider)) {
                targetControls.focusProvider = mirabufSceneObject
            }
            closeModal(CloseType.OVERWRITE)
        },
        [closeModal, miraType]
    )
    const onBeforeAccept = useCallback(async () => {
        if (!selectedFile || miraType === undefined) return

        const buffer = await selectedFile.arrayBuffer()
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)

        const progressHandle = new ProgressHandle(`Importing ${selectedFile.name}`)
        try {
            let mirabufSceneObject: MirabufSceneObject | undefined
            const isURDF = isURDFFile(selectedFile.name)
            if (isURDF) {
                const inputHash = await hashBuffer(buffer)
                const uuid = uuidV4({ random: hexStringToUint8Array(inputHash).slice(0, 16) })
                const { assembly, foundDrivetrain } = await loadURDF(buffer, selectedFile.name, progressHandle)
                // Default is the assembly name, which is often Assembly 1 or something else similarly non-descriptive. People will (likely) name the files something useful
                assembly.info!.name = selectedFile.name.split(".")[0]
                assembly.info!.GUID = uuid

                progressHandle.update("Configuring wheels", URDFImportProgressBar.CONFIG)

                mirabufSceneObject = await createMirabuf(inputHash, assembly, progressHandle)
                if (mirabufSceneObject) {
                    finalizeSceneObject(mirabufSceneObject)
                    if (!foundDrivetrain) {
                        await new Promise<void>(resolve => {
                            openPanel(WheelAssignmentPanel, undefined, modal, {
                                onClose: () => {
                                    resolve()
                                },
                            })
                        })
                    }
                    const res = await MirabufCachingService.storeAssemblyInCache(assembly, { miraType })

                    if (res == null) {
                        console.warn("Caching URDF failed!")
                    }

                    openPanel(InitialConfigPanel, undefined, modal)
                }

                progressHandle.done("Import complete!")
            } else {
                const result = await MirabufCachingService.cacheLocalAndReturn(buffer, miraType)
                if (!result) {
                    globalOpenModal(ImportLocalMirabufModal, {
                        configurationType: miraTypeToConfigType(miraType),
                    })
                    return
                }
                mirabufSceneObject = await createMirabuf(result.cacheInfo.hash, result.assembly, undefined)
                if (mirabufSceneObject) {
                    finalizeSceneObject(mirabufSceneObject)
                    if (mirabufSceneObject.miraType == MiraType.ROBOT) {
                        openPanel(InitialConfigPanel, undefined, modal)
                    }
                }
            }

            if (!mirabufSceneObject) {
                globalOpenModal(ImportLocalMirabufModal, {
                    configurationType: miraTypeToConfigType(miraType),
                })
            }
        } catch (e) {
            console.error("[Import]", e)
            progressHandle.fail("Import failed!")
            openModal(ImportLocalMirabufModal, {
                configurationType: miraTypeToConfigType(miraType),
                errorMessage: e instanceof Error ? e.message : "An unknown error occurred during import.",
            })
        } finally {
            setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500)
        }
    }, [openPanel, openModal, miraType, modal, selectedFile, finalizeSceneObject])

    useEffect(() => {
        configureScreen(
            modal!,
            { title: "Import from File", hideAccept: selectedFile === undefined || miraType === undefined },
            { onBeforeAccept, onCancel }
        )
    }, [configureScreen, selectedFile, miraType, modal, onBeforeAccept, onCancel])

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
