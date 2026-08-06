import type React from "react"
import { useCallback, useEffect, useId, useMemo, useState } from "react"
import type { PanelImplProps } from "@/components/Panel.tsx"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import World from "@/systems/World.ts"
import WheelAssignment from "@/components/UserModelConfig/WheelAssignment.tsx"
import { Divider, Stack } from "@mui/material"
import { Button, ProgressButton, SynthesisIcons, TooltipButton } from "@/components/StyledComponents.tsx"
import DrivetrainConfig from "@/components/UserModelConfig/DrivetrainConfig.tsx"
import DeleteParts from "@/components/UserModelConfig/DeleteParts.tsx"
import { applyModelConfigChanges } from "@/systems/scene/ApplyModelConfig.ts"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts";

export interface SubpanelProps {
    setDisableNextMessage: (v: string | null) => void
    sceneObject: MirabufSceneObject
    pauseRef: string
}

const screens: { title: string; component: React.FC<SubpanelProps> }[] = [
    { title: "Delete Parts", component: DeleteParts },
    { title: "Assign Wheels", component: WheelAssignment },
    { title: "Drivetrain", component: DrivetrainConfig },

]

const ModelConfigPanel: React.FC<PanelImplProps<void, { sceneObject: MirabufSceneObject }>> = ({ panel }) => {
    const { configureScreen, closePanel } = useUIContext()
    const [screen, setScreen] = useState<number>(0)
    const [disableNextMessage, setDisableNextMessage] = useState<string | null>(null)

    const { sceneObject: originalSceneObject } = panel!.props.custom

    const [sceneObject, setSceneObject] = useState<MirabufSceneObject>(originalSceneObject)

    const title = useMemo(() => screens[screen].title, [screen])
    const ScreenComponent = useMemo(() => screens[screen].component, [screen])

    const pauseHandle = useId()
    useEffect(() => {
        World.physicsSystem.holdPause(pauseHandle)
        return () => {
            World.physicsSystem.releasePause(pauseHandle)
        }
    }, [pauseHandle])

    const apply = useCallback(async () => {
        await applyModelConfigChanges()

        const newObj = World.sceneRenderer.sceneObjects.get(originalSceneObject.id)
        if (!newObj || !(newObj instanceof MirabufSceneObject)) {
            return
        }
        setSceneObject(newObj)
        return newObj
    }, [originalSceneObject])

    useEffect(() => {
        configureScreen(panel!, { title, hideCancel: true, hideAccept: true }, {})
    }, [configureScreen, panel, title])

    const closeCallback = useCallback(async () => {
        closePanel(panel!.id, CloseType.ACCEPT)
    }, [panel, closePanel, apply])

    return (
        <Stack gap={2}>
            <ScreenComponent
                setDisableNextMessage={setDisableNextMessage}
                sceneObject={sceneObject}
                pauseRef={pauseHandle}
            />
            <Divider />
            {screen == 0 && (
                <ProgressButton
                    color="secondary"
                    refreshLabel={"Applying..."}
                    sx={{ px: 4, flexGrow: 2 }}
                    onClick={async () => {
                        await apply()
                        setScreen(screen + 1)
                    }}
                >
                    Apply
                </ProgressButton>
            )}
            {screen == screens.length - 1 && (
                <Button color="secondary" sx={{ px: 4, flexGrow: 2 }} onClick={closeCallback}>
                    Apply
                </Button>
            )}
            <Stack direction={"row"} gap={1}>
                <Button
                    variant="outlined"
                    color="secondary"
                    size={"medium"}
                    sx={{ px: 4 }}
                    disabled={true}
                    onClick={() => setScreen(screen - 1)}
                >
                    <SynthesisIcons.LEFT_ARROW_LARGE />
                </Button>
                <TooltipButton
                    variant="outlined"
                    size={"medium"}
                    color="secondary"
                    sx={{ px: 4 }}
                    tooltip={disableNextMessage ?? undefined}
                    disabled={screen == 1 || disableNextMessage != null}
                    onClick={() => setScreen(screen + 1)}
                >
                    <SynthesisIcons.RIGHT_ARROW_LARGE />
                </TooltipButton>
            </Stack>
        </Stack>
    )
}

export default ModelConfigPanel
