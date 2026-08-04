import type React from "react"
import { useEffect, useId, useMemo, useState } from "react"
import type { PanelImplProps } from "@/components/Panel.tsx"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import World from "@/systems/World.ts"
import WheelAssignment from "@/components/UserModelConfig/WheelAssignment.tsx"
import { Divider, Stack } from "@mui/material"
import { Button, IconButton, ProgressButton, SynthesisIcons } from "@/components/StyledComponents.tsx"
import DrivetrainConfig from "@/components/UserModelConfig/DrivetrainConfig.tsx"

const screens: { title: string; component: React.ReactElement }[] = [
    { title: "Assign Wheels", component: <WheelAssignment /> },
    { title: "Drivetrain", component: <DrivetrainConfig /> },
    { title: "Delete Parts", component: <></> },
]

const ModelConfigPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [screen, setScreen] = useState<number>(0)
    const title = useMemo(() => screens[screen].title, [screen])
    const screenComponent = useMemo(() => screens[screen].component, [screen])

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

    return (
        <Stack gap={2}>
            {screenComponent}
            <Divider />
            {screen == screens.length - 1 && (
                <ProgressButton
                    color="secondary"
                    refreshLabel={"Applying..."}
                    sx={{ px: 4, flexGrow: 2 }}
                    onClick={() => World.wheelAssignmentMode.apply()}
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
                <Button
                    variant="outlined"
                    size={"medium"}
                    color="secondary"
                    sx={{ px: 4 }}
                    disabled={screen == screens.length - 1}
                    onClick={() => setScreen(screen + 1)}
                >
                    <SynthesisIcons.RIGHT_ARROW_LARGE />
                </Button>
            </Stack>
        </Stack>
    )
}

export default ModelConfigPanel
