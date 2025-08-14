import { Stack, Typography } from "@mui/material"
import type React from "react"
import { useEffect } from "react"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

export type ConfirmModalCustomProps = {
    message: string
}

const ConfirmModal: React.FC<ModalImplProps<void, ConfirmModalCustomProps>> = ({ modal }) => {
    const { configureScreen } = useUIContext()

    useEffect(() => {
        if (!modal) return
        configureScreen(
            modal,
            {
                title: modal.props.title ?? "Confirm",
                acceptText: modal.props.acceptText ?? "Confirm",
                cancelText: modal.props.cancelText ?? "Cancel",
                hideAccept: false,
                hideCancel: false,
                allowClickAway: modal.props.allowClickAway ?? true,
            },
            {}
        )
    }, [modal, configureScreen])

    if (!modal) return null

    const { message } = modal.props.custom

    return (
        <Stack component="div" gap={1} sx={{ minWidth: 320, maxWidth: 500 }}>
            <Typography sx={{ userSelect: "none" }}>{message}</Typography>
        </Stack>
    )
}

export default ConfirmModal
