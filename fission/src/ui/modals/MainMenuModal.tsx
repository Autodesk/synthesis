import { Stack } from "@mui/material"
import type React from "react"
import { useLayoutEffect } from "react"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import type { ModalImplProps } from "../components/Modal"
import { Button } from "../components/StyledComponents"
import { useStateContext } from "../helpers/StateProviderHelpers"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"
import { spawnCachedMira } from "../panels/mirabuf/ImportMirabufPanel"

interface MainMenuCustomProps {
    startSingleplayerCallback: () => void
    startMultiplayerCallback: () => void
}

const MainMenuModal: React.FC<ModalImplProps<void, MainMenuCustomProps>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()
    const { setIsMainMenuOpen } = useStateContext()

    const { startSingleplayerCallback, startMultiplayerCallback } = modal!.props.custom!

    useLayoutEffect(() => {
        setIsMainMenuOpen(true)
        configureScreen(modal!, { title: "Welcome", hideAccept: true, hideCancel: true, allowClickAway: false }, {})

        return () => {
            setIsMainMenuOpen(false)
        }
    }, [])
    return (
        <Stack gap={1}>
            <Button
                onClick={() => {
                    closeModal(CloseType.Accept)
                    startSingleplayerCallback()
                }}
                fullWidth={true}
                className="mt-1 mb-3"
            >
                Singleplayer
            </Button>

            <Button
                onClick={() => {
                    closeModal(CloseType.Accept)
                    startMultiplayerCallback()
                }}
                fullWidth={true}
                className="mt-1 mb-3"
            >
                Multiplayer
            </Button>

            <Button
                onClick={async () => {
                    closeModal(CloseType.Accept)
                    startSingleplayerCallback()

                    const [field, robot] = await Promise.all([
                        MirabufCachingService.cacheRemote("/api/mira/fields/FRC Field 2023_v7.mira", MiraType.FIELD),
                        MirabufCachingService.cacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT),
                    ])

                    if (field) await spawnCachedMira(field, MiraType.FIELD)
                    if (robot) await spawnCachedMira(robot, MiraType.ROBOT)
                }}
                className="my-1"
            >
                Load Default Scene
            </Button>
        </Stack>
    )
}

export default MainMenuModal
