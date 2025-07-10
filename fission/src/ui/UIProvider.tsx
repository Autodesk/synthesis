import { useSnackbar } from "notistack"
import type { EnqueueSnackbar } from "notistack"
import { createContext, useState } from "react"
import type React from "react"
import type { ReactElement, ReactNode } from "react"
import { v4 as uuidv4 } from "uuid"

export type UIProviderProps = {
    children?: ReactNode
}

export enum CloseType {
    Accept = 0,
    Cancel = 1,
    Overwrite = 2,
}

export type UIScreenProps = Partial<{
    onClose: (closeType: CloseType) => void
    onCancel: () => void
    onAccept: () => void
    htmlProps: string
}>

export interface UIScreen {
    id: string
    parent: UIScreen
    content: React.FC<unknown>
    props: UIScreenProps
}

export type PanelPosition =
    | "top-left"
    | "top"
    | "top-right"
    | "left"
    | "center"
    | "right"
    | "bottom-left"
    | "bottom"
    | "bottom-right"

export interface Modal extends UIScreen {
    allowClickAway: boolean
}

export interface Panel extends UIScreen {
    position: PanelPosition
}

export type OpenModalFn = (contents: ReactElement, parent?: UIScreen, props?: UIScreenProps) => string
export type OpenPanelFn = (
    contents: ReactElement,
    parent?: UIScreen,
    position?: PanelPosition,
    props?: UIScreenProps
) => string
export type CloseModalFn = (closeType: CloseType) => void
export type ClosePanelFn = (id: string, closeType: CloseType) => void

export type UIContextProps = {
    modal?: Modal
    panels: Panel[]
    openModal: OpenModalFn
    openPanel: OpenPanelFn
    closeModal: CloseModalFn
    closePanel: ClosePanelFn
    enqueueSnackbar: EnqueueSnackbar
}

export const UIContext = createContext<UIContextProps>({
    panels: [],
    openModal: (_content, _parent, _props = {}) => "",
    openPanel: (_content, _parent, _position = "center", _props = {}) => "",
    closeModal: () => {},
    closePanel: _id => {},
    enqueueSnackbar: _msg => "",
})

export const UIProvider: React.FC<UIProviderProps> = ({ children }) => {
    const [modal, setModal] = useState<Modal | undefined>(undefined)
    const [panels, setPanels] = useState<Panel[]>([])

    const { enqueueSnackbar } = useSnackbar()

    // TODO: add support for modal-specific props (i.e. allowClickAway)
    const openModal: OpenModalFn = (content: ReactElement, parent?: UIScreen, props: UIScreenProps = {}) => {
        const id = uuidv4()
        const modal = {
            id,
            parent,
            content,
            props,
        } as Modal
        modal?.props.onClose?.(CloseType.Overwrite)
        setModal(modal)
        return id
    }

    const openPanel: OpenPanelFn = (
        content: ReactElement,
        parent?: UIScreen,
        position: PanelPosition = "center",
        props: UIScreenProps = {}
    ) => {
        console.log("PROPS", props)
        const id = uuidv4()
        const panel = {
            id,
            parent,
            content,
            position,
            props,
        } as Panel
        setPanels([...panels, panel])
        return id
    }

    const closeCallbacks = (elem: Panel | Modal, closeType: CloseType) => {
        elem.props.onClose?.(closeType)
        switch (closeType) {
            case CloseType.Accept:
                elem.props.onAccept?.()
                break
            case CloseType.Cancel:
                elem.props.onCancel?.()
                break
            default:
                break
        }
    }

    const closeModal = (closeType: CloseType) => {
        if (modal) closeCallbacks(modal, closeType)
        setModal(undefined)
    }

    const closePanel = (id: string, closeType: CloseType) => {
        const panel = panels.find((p: Panel) => p.id === id)
        if (panel) closeCallbacks(panel, closeType)
        setPanels(panels.filter((p: Panel) => p.id !== id))
    }

    return (
        <UIContext.Provider
            value={{
                modal,
                panels,
                openModal,
                openPanel,
                closeModal,
                closePanel,
                enqueueSnackbar,
            }}
        >
            {children}
        </UIContext.Provider>
    )
}
