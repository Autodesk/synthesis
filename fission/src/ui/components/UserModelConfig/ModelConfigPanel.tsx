import type React from "react"
import { useCallback, useEffect, useId, useMemo, useState } from "react"
import type { PanelImplProps } from "@/components/Panel.tsx"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import World from "@/systems/World.ts"
import WheelAssignment from "@/components/UserModelConfig/WheelAssignment.tsx"
import { Divider, Stack } from "@mui/material"
import { Button, ProgressButton, SynthesisIcons } from "@/components/StyledComponents.tsx"
import DrivetrainConfig from "@/components/UserModelConfig/DrivetrainConfig.tsx"
import DeleteParts from "@/components/UserModelConfig/DeleteParts.tsx"
import { applyModelConfigChanges } from "@/systems/scene/ApplyModelConfig.ts"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import { globalAddToast } from "@/components/GlobalUIControls.ts"

export interface SubpanelProps {
    sceneObject: MirabufSceneObject
    pauseRef: string
}

interface Screen {
    title: string
    component: React.FC<SubpanelProps>
    disableBack?: boolean
    disableNext?: boolean
    showApply?: boolean
    showSave?: boolean
}

const screens: Screen[] = [
    { title: "Delete Parts", component: DeleteParts, disableBack: true },
    { title: "Assign Wheels", component: WheelAssignment, disableNext: true, showApply: true },
    { title: "Drivetrain", component: DrivetrainConfig, disableNext: true, showSave: true },
]

const ModelConfigPanel: React.FC<PanelImplProps<void, { sceneObject: MirabufSceneObject; hasDrivetrain: boolean }>> = ({
    panel,
}) => {
    const { configureScreen, closePanel } = useUIContext()
    const [screenIndex, setScreenIndex] = useState<number>(0)

    const { sceneObject: originalSceneObject } = panel!.props.custom

    const [sceneObject, setSceneObject] = useState<MirabufSceneObject>(originalSceneObject)

    const screen = useMemo(() => screens[screenIndex], [screenIndex])
    const ScreenComponent = useMemo(() => screens[screenIndex].component, [screenIndex])

    const pauseHandle = useId()
    useEffect(() => {
        World.physicsSystem.holdPause(pauseHandle)
        return () => {
            World.physicsSystem.releasePause(pauseHandle)
        }
    }, [pauseHandle])

    const apply = useCallback(
        async (advance: boolean) => {
            if (World.wheelAssignmentMode.pendingWheels.size < 4) {
                globalAddToast("warning", "Model Config", "Must select at least 4 wheels!")
            }
            const success = await applyModelConfigChanges()
            if (success) {
                globalAddToast(
                    "success",
                    "Model Config",
                    "Applied the pending changes and rebuilt the affected assembly."
                )
            } else {
                globalAddToast("error", "Model Config", "Failed to rebuild assembly.")
                return
            }

            const newObj = World.sceneRenderer.sceneObjects.get(originalSceneObject.id)
            if (!newObj || !(newObj instanceof MirabufSceneObject)) {
                return
            }
            setSceneObject(newObj)
            if (advance) {
                setScreenIndex(s => s + 1)
            }
        },
        [originalSceneObject]
    )

    useEffect(() => {
        configureScreen(
            panel!,
            {
                title: `Model Config - ${screen.title}`,
                hideCancel: true,
                hideAccept: true,
                blocking: true,
                blockingMessage: "Finish model config first!",
            },
            {
                onCancel: () => {
                    World.wheelAssignmentMode.cancel()
                    World.partDeletionMode.cancel()
                    World.sceneRenderer.removeSceneObject(sceneObject.id)
                },
            }
        )
    }, [configureScreen, panel, screen, sceneObject.id])

    const closeCallback = useCallback(async () => {
        closePanel(panel!.id, CloseType.ACCEPT)
    }, [panel, closePanel, apply])

    return (
        <Stack gap={2}>
            <ScreenComponent sceneObject={sceneObject} pauseRef={pauseHandle} />
            <Divider />
            {screen.showApply && (
                <ProgressButton
                    color="secondary"
                    refreshLabel={"Applying..."}
                    sx={{ px: 4, flexGrow: 2 }}
                    onClick={() => apply(true)}
                >
                    Apply
                </ProgressButton>
            )}
            {screen.showSave && (
                <Button color="secondary" sx={{ px: 4, flexGrow: 2 }} onClick={closeCallback}>
                    Save
                </Button>
            )}
            <Stack direction={"row"} gap={1} justifyContent={"center"}>
                <Button
                    variant="outlined"
                    color="error"
                    size={"medium"}
                    sx={{ px: 4, flex: "auto" }}
                    onClick={() => {
                        closePanel(panel!.id, CloseType.CANCEL)
                    }}
                >
                    Abort
                </Button>
                <Button
                    variant="outlined"
                    color="secondary"
                    size={"medium"}
                    sx={{ px: 4, flex: "auto" }}
                    disabled={screen.disableBack}
                    onClick={() => setScreenIndex(screenIndex - 1)}
                >
                    <SynthesisIcons.LEFT_ARROW_LARGE />
                </Button>
                <Button
                    variant="outlined"
                    size={"medium"}
                    color="secondary"
                    sx={{ px: 4, flex: "auto" }}
                    disabled={screen.disableNext}
                    onClick={() => setScreenIndex(screenIndex + 1)}
                >
                    <SynthesisIcons.RIGHT_ARROW_LARGE />
                </Button>
            </Stack>
        </Stack>
    )
}

export default ModelConfigPanel
