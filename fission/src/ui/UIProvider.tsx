import { useSnackbar } from "notistack"
import type { EnqueueSnackbar, VariantType } from "notistack"
import { createContext, useCallback, useState } from "react"
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
export type AddToastFn = (variant: VariantType, title: string) => void

export type UIContextProps = {
    modal?: Modal
    panels: Panel[]
    openModal: OpenModalFn
    openPanel: OpenPanelFn
    closeModal: CloseModalFn
    closePanel: ClosePanelFn
    addToast: AddToastFn
}

export const UIContext = createContext<UIContextProps>({
    panels: [],
    openModal: (_content, _parent, _props = {}) => "",
    openPanel: (_content, _parent, _position = "center", _props = {}) => "",
    closeModal: () => {},
    closePanel: _id => {},
    addToast: (_variant, _msg) => "",
})

export const UIProvider: React.FC<UIProviderProps> = ({ children }) => {
    const [modal, setModal] = useState<Modal | undefined>(undefined)
    const [panels, setPanels] = useState<Panel[]>([])

    const { enqueueSnackbar } = useSnackbar()

    // TODO: add support for modal-specific props (i.e. allowClickAway)
    const openModal: OpenModalFn = useCallback(
        (content: ReactElement, parent?: UIScreen, props: UIScreenProps = {}) => {
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
        },
        []
    )

    const openPanel: OpenPanelFn = useCallback(
        (content: ReactElement, parent?: UIScreen, position: PanelPosition = "center", props: UIScreenProps = {}) => {
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
        },
        []
    )

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

    const closeModal = useCallback((closeType: CloseType) => {
        if (modal) closeCallbacks(modal, closeType)
        setModal(undefined)
    }, [])

    const closePanel = useCallback((id: string, closeType: CloseType) => {
        setPanels(p => {
            const panel = p.find((p: Panel) => p.id === id)
            if (panel) closeCallbacks(panel, closeType)
            return p.filter((pnl: Panel) => pnl.id !== id)
        })
    }, [])

    const addToast = useCallback((variant: VariantType, title: string) => {
        enqueueSnackbar(title, { variant })
    }, [])

    return (
        <UIContext.Provider
            value={{
                modal,
                panels,
                openModal,
                openPanel,
                closeModal,
                closePanel,
                addToast,
            }}
        >
            {children}
        </UIContext.Provider>
    )
}
