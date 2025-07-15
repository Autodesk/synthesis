import { Button, Card, CardActions, CardContent, Modal as MUIModal } from "@mui/material"
import React, { useContext } from "react"
import type { Modal as ModalType, Panel as PanelType } from "../UIProvider"
import { CloseType, UIContext } from "../UIProvider"

export type ModalImplProps = Partial<{
    modal: ModalType
    parent: PanelType | ModalType
}>

interface ModalProps {
    children?: React.FC<ModalImplProps>
    modal: ModalType
    parent?: ModalType | PanelType
}

export const Modal: React.FC<ModalProps> = ({ children, modal, parent }) => {
    const { closeModal } = useContext(UIContext)
    return (
        <MUIModal open={modal !== undefined} onClose={closeModal}>
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
                <CardContent>
                    {React.Children.map(children, child => {
                        if (React.isValidElement(child)) return React.cloneElement(child, { modal, parent })
                    })}
                </CardContent>
                <CardActions>
                    <Button onClick={() => closeModal(CloseType.Cancel)} variant="outlined" color="error">
                        Close
                    </Button>
                    <Button onClick={() => closeModal(CloseType.Accept)} variant="contained" color="success">
                        Accept
                    </Button>
                </CardActions>
            </Card>
        </MUIModal>
    )
}
