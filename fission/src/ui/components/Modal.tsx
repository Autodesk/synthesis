import { Button, Card, CardActions, CardContent, CardHeader, Modal as MUIModal } from "@mui/material"
import React, { useEffect, useState, type ReactElement } from "react"
import type { Modal as ModalType, Panel as PanelType } from "../helpers/UIProviderHelpers"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"

export type ModalImplProps<T> = Partial<{
    modal: ModalType<T>
    parent: PanelType<T> | ModalType<T>
}>

interface ModalElementProps<T> {
    children?: ReactElement<ModalImplProps<T>>
    modal: ModalType<T>
    parent?: ModalType<T> | PanelType<T>
}

export const Modal = <T,>({ children, modal, parent }: ModalElementProps<T>) => {
    const { closeModal } = useUIContext()
    const props = modal.props
    const [_, refresh] = useState(false)

    // biome-ignore lint/correctness/useExhaustiveDependencies: to refresh on configure
    useEffect(() => {
        refresh(x => !x)
    }, [modal.props.configured])

    return (
        <MUIModal
            open={modal !== undefined}
            onClose={() => {
                if (props.allowClickAway) closeModal(CloseType.Cancel)
            }}
        >
            <Card
                // TODO: come up with a better solution than this
                sx={{
                    display: modal.props.configured ? "" : "none",
                    position: "absolute",
                    top: "50%",
                    left: "50%",
                    transform: "translate(-50%, -50%)",
                    p: 4,
                }}
            >
                {props.title && <CardHeader title={props.title} />}
                <CardContent>
                    {React.Children.map(children, child => {
                        if (React.isValidElement(child)) return React.cloneElement(child, { modal, parent })
                    })}
                </CardContent>
                {((props.hideCancel !== undefined && !props.hideCancel) ||
                    (props.hideAccept !== undefined && !props.hideAccept)) && (
                    <CardActions>
                        {props.hideCancel !== undefined && !props.hideCancel && (
                            <Button onClick={() => closeModal(CloseType.Cancel)} variant="outlined" color="error">
                                {props.cancelText ?? "Cancel"}
                            </Button>
                        )}
                        {props.hideAccept !== undefined && !props.hideAccept && (
                            <Button onClick={() => closeModal(CloseType.Accept)} variant="contained" color="success">
                                {props.acceptText ?? "Accept"}
                            </Button>
                        )}
                    </CardActions>
                )}
            </Card>
        </MUIModal>
    )
}
