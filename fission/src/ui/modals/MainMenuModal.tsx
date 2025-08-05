import { Button, Stack } from "@mui/material"
import type React from "react"
import { useLayoutEffect } from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import type { ModalImplProps } from "../components/Modal"
import { useStateContext } from "../helpers/StateProviderHelpers"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"

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
    }, [configureScreen, modal, setIsMainMenuOpen])
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
