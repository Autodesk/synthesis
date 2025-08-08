import { Stack, Typography } from "@mui/material"
import type React from "react"
import { useEffect } from "react"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import type { ModalImplProps } from "@/ui/components/Modal"

export type ConfirmModalCustomProps = {
    title?: string
    message: string
    acceptText?: string
    cancelText?: string
    allowClickAway?: boolean
    onConfirm?: () => void
    onCancel?: () => void
}

const ConfirmModal: React.FC<ModalImplProps<void, ConfirmModalCustomProps>> = ({ modal }) => {
    const { configureScreen } = useUIContext()

    useEffect(() => {
        if (!modal) return
        const custom = modal.props.custom
        configureScreen(
            modal,
            {
                title: custom.title ?? "Confirm",
                acceptText: custom.acceptText ?? "Confirm",
                cancelText: custom.cancelText ?? "Cancel",
                hideAccept: false,
                hideCancel: false,
                allowClickAway: custom.allowClickAway ?? true,
            },
            {
                onBeforeAccept: () => {
                    custom.onConfirm?.()
                },
                onCancel: () => custom.onCancel?.(),
            }
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
