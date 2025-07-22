import { Button, Card, CardActions, CardContent, CardHeader, Modal as MUIModal } from "@mui/material"
import React, { type ReactElement } from "react"
import type { Modal as ModalType, Panel as PanelType } from "../UIProvider"
import { CloseType, useUIContext } from "../UIProvider"

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

    return (
        <MUIModal
            open={modal !== undefined}
            onClose={() => {
                if (props.allowClickAway) closeModal(CloseType.Cancel)
            }}
        >
            <Card
                sx={{
                    position: "absolute",
                    top: "50%",
                    left: "50%",
                    transform: "translate(-50%, -50%)",
                    maxWidth: 400,
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
