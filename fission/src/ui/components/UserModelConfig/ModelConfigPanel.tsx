import type React from "react"
import { useEffect, useId, useMemo, useState } from "react"
import type { PanelImplProps } from "@/components/Panel.tsx"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import World from "@/systems/World.ts"
import WheelAssignment from "@/components/UserModelConfig/WheelAssignment.tsx"
import { Divider, Stack } from "@mui/material"
import { Button, ProgressButton } from "@/components/StyledComponents.tsx"
import DrivetrainConfig from "@/components/UserModelConfig/DrivetrainConfig.tsx"
import DeleteParts from "@/components/UserModelConfig/DeleteParts.tsx"

const screens: { title: string; component: React.ReactElement }[] = [
    { title: "Assign Wheels", component: <WheelAssignment /> },
    { title: "Drivetrain", component: <DrivetrainConfig /> },
    { title: "Delete Parts", component: <DeleteParts /> },
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
            <Stack direction={"row"} gap={1}>
                <Button
                    color="secondary"
                    sx={{ px: 4, flexBasis: 1 }}
                    disabled={screen == 0}
                    onClick={() => setScreen(screen - 1)}
                >
                    Back
                </Button>
                {screen == screens.length - 1 ? (
                    <ProgressButton
                        color="secondary"
                        refreshLabel={"Applying..."}
                        sx={{ px: 4, flexBasis: 1 }}
                        onClick={async () => {
                            await World.wheelAssignmentMode.apply()
                            await World.partDeletionMode.apply()
                        }}
                    >
                        Apply
                    </ProgressButton>
                ) : (
                    <Button color="secondary" sx={{ px: 4, flexBasis: 1 }} onClick={() => setScreen(screen + 1)}>
                        Next
                    </Button>
                )}
            </Stack>
        </Stack>
    )
}

export default ModelConfigPanel
