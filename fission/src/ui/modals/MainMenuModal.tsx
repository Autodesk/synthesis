import { Stack } from "@mui/material"
import { Button } from "../components/StyledComponents"
import type React from "react"
import { useLayoutEffect } from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import type { ModalImplProps } from "../components/Modal"
import { useStateContext } from "../helpers/StateProviderHelpers"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"
import { spawnCachedMira } from "../panels/mirabuf/ImportMirabufPanel"

interface MainMenuCustomProps {
    startSingleplayerCallback: () => void
}

const MainMenuModal: React.FC<ModalImplProps<void, MainMenuCustomProps>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()
    const { setIsMainMenuOpen } = useStateContext()

    const { startSingleplayerCallback } = modal!.props.custom!

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
                className="my-1"
            >
                Singleplayer
            </Button>
            <Button
                onClick={() => {
                    closeModal(CloseType.Accept)
                    startSingleplayerCallback()
                    Promise.all([
                        MirabufCachingService.cacheRemote("/api/mira/fields/FRC Field 2023_v7.mira", MiraType.FIELD),
                        MirabufCachingService.cacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT),
                    ]).then(([cachedField, cachedRobot]) => {
                        if (cachedField && cachedRobot) {
                            spawnCachedMira(cachedField, MiraType.FIELD)
                            spawnCachedMira(cachedRobot, MiraType.ROBOT)
                        }
                    })
                }}
                className="my-1"
            >
                Load Default
            </Button>
            <Button
                onClick={() => {
                    globalAddToast("error", "Not Supported", "Multiplayer is not yet supported. Come back soon!")
                }}
                fullWidth={true}
                className="mt-1 mb-3"
            >
                Multiplayer
            </Button>
        </Stack>
    )
}

export default MainMenuModal
