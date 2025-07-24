import { Button, Stack } from "@mui/material"
import type React from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import type { ModalImplProps } from "../components/Modal"
import { CloseType, useUIContext } from "../UIProvider"
import { useEffect, useLayoutEffect } from "react"
import { useStateContext } from "../StateProvider"

const MainMenuModal: React.FC<ModalImplProps<void> & { startSingleplayerCallback: () => void }> = ({
    modal,
    startSingleplayerCallback,
}) => {
    const { configureScreen, closeModal } = useUIContext()
    const { setIsMainMenuOpen } = useStateContext()
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
