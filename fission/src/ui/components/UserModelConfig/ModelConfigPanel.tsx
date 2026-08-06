import type React from "react"
import { useCallback, useEffect, useId, useMemo, useState } from "react"
import type { PanelImplProps } from "@/components/Panel.tsx"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import World from "@/systems/World.ts"
import WheelAssignment from "@/components/UserModelConfig/WheelAssignment.tsx"
import { Divider, Stack } from "@mui/material"
import { Button, ProgressButton, SynthesisIcons, TooltipButton } from "@/components/StyledComponents.tsx"
import DrivetrainConfig from "@/components/UserModelConfig/DrivetrainConfig.tsx"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"

export interface SubpanelProps {
    setDisableNextMessage: (v: string | null) => void
    sceneObject: MirabufSceneObject
}

const screens: { title: string; component: React.FC<SubpanelProps> | null }[] = [
    { title: "Assign Wheels", component: WheelAssignment },
    { title: "Drivetrain", component: DrivetrainConfig },
]

const ModelConfigPanel: React.FC<PanelImplProps<void, { sceneObject: MirabufSceneObject }>> = ({ panel }) => {
    const { configureScreen, closePanel } = useUIContext()
    const [screen, setScreen] = useState<number>(0)
    const [disableNextMessage, setDisableNextMessage] = useState<string | null>(null)

    const { sceneObject } = panel!.props.custom

    const title = useMemo(() => screens[screen].title, [screen])
    const ScreenComponent = useMemo(() => screens[screen].component, [screen])

    const pauseHandle = useId()
    useEffect(() => {
        World.physicsSystem.holdPause(pauseHandle)
        return () => {
            World.physicsSystem.releasePause(pauseHandle)
        }
    }, [pauseHandle])

    useEffect(() => {
        configureScreen(panel!, { title, hideCancel: true, hideAccept: true }, {})
    }, [configureScreen, panel, title])

    const closeCallback = useCallback(async () => {
        await World.wheelAssignmentMode.apply()
        closePanel(panel!.id, CloseType.ACCEPT)
    }, [panel, closePanel])

    return (
        <Stack gap={2}>
            {ScreenComponent != null && (
                <ScreenComponent setDisableNextMessage={setDisableNextMessage} sceneObject={sceneObject} />
            )}
            <Divider />
            {screen == screens.length - 1 && (
                <ProgressButton
                    color="secondary"
                    refreshLabel={"Applying..."}
                    sx={{ px: 4, flexGrow: 2 }}
                    onClick={closeCallback}
                >
                    Apply
                </ProgressButton>
            )}
            <Stack direction={"row"} gap={1}>
                <Button
                    variant="outlined"
                    color="secondary"
                    size={"medium"}
                    sx={{ px: 4 }}
                    disabled={screen == 0}
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
                    disabled={screen == screens.length - 1 || disableNextMessage != null}
                    onClick={() => setScreen(screen + 1)}
                >
                    <SynthesisIcons.RIGHT_ARROW_LARGE />
                </TooltipButton>
            </Stack>
        </Stack>
    )
}

export default ModelConfigPanel
