import { Card, CardActions, CardContent, CardHeader, Modal as MUIModal } from "@mui/material"
import React, { type ReactElement } from "react"
import type { Modal as ModalType, Panel as PanelType } from "../helpers/UIProviderHelpers"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"
import { Button } from "./StyledComponents"

export type ModalImplProps<T, P> = Partial<{
    modal: ModalType<T, P>
    parent: PanelType<T, P> | ModalType<T, P>
}>

// biome-ignore-start lint/suspicious/noExplicitAny: need to be able to extend
interface ModalElementProps<T, P> {
    children?: ReactElement<ModalImplProps<any, any>>
    modal: ModalType<T, P>
    parent?: ModalType<any, any> | PanelType<any, any>
}
// biome-ignore-end lint/suspicious/noExplicitAny: need to be able to extend

export const Modal = <T, P>({ children, modal, parent }: ModalElementProps<T, P>) => {
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
                    display: modal.props.configured ? "flex" : "none",
                    position: "absolute",
                    top: "50%",
                    left: "50%",
                    transform: "translate(-50%, -50%)",
                    p: 0,
                    maxHeight: "85vh",
                    minWidth: "20vw",
                    bgcolor: "background.default",
                    flexDirection: "column",
                    backgroundColor: "#2e2e2e",
                    boxShadow: 6,
                }}
            >
                {props.title && (
                    <CardHeader
                        title={props.title}
                        className="select-none"
                        titleTypographyProps={{ variant: "h5" }}
                        sx={{
                            cursor: "move",
                            py: 1,
                            px: 2,
                            borderBottom: theme => `1px solid ${theme.palette.divider}`,
                            position: "sticky",
                            top: 0,
                            zIndex: 1,
                            backgroundColor: "inherit",
                        }}
                    />
                )}
                <CardContent sx={{ p: 2, flex: "1 1 auto", overflowY: "auto" }}>
                    {React.Children.map(children, child => {
                        if (React.isValidElement(child)) return React.cloneElement(child, { modal, parent })
                    })}
                </CardContent>
                {(!props.hideCancel || !props.hideAccept) && (
                    <CardActions
                        sx={{
                            position: "sticky",
                            bottom: 0,
                            zIndex: 1,
                            bgcolor: "inherit",
                            borderTop: theme => `1px solid ${theme.palette.divider}`,
                            p: 2,
                        }}
                    >
                        {!props.hideCancel && (
                            <Button onClick={() => closeModal(CloseType.Cancel)} variant="outlined" color="secondary">
                                {props.cancelText ?? "Cancel"}
                            </Button>
                        )}
                        {!props.hideAccept && (
                            <Button
                                onClick={() => closeModal(CloseType.Accept)}
                                variant="contained"
                                color="primary"
                                disabled={props.disableAccept}
                            >
                                {props.acceptText ?? "Accept"}
                            </Button>
                        )}
                    </CardActions>
                )}
            </Card>
        </MUIModal>
    )
}
