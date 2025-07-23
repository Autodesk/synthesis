import type { VariantType } from "notistack"
import { useSnackbar } from "notistack"
import type React from "react"
import type { ReactElement, ReactNode } from "react"
import { createContext, useCallback, useContext, useState } from "react"
import { v4 as uuidv4 } from "uuid"
import { UICallback } from "./UICallbacks"

export type UIProviderProps = {
    children?: ReactNode
}

export enum CloseType {
    Accept = 0,
    Cancel = 1,
    Overwrite = 2,
}

interface UIScreenCallbacks<T> {
    onClose?: () => void
    onCancel?: () => void
    onBeforeAccept?: () => T
    onAccept?: (arg: T) => void
}

/**
 *  Props for generic UIScreen
 */
export interface UIScreenProps {
    title?: string
    htmlProps?: string
    hideCancel?: boolean
    hideAccept?: boolean
    cancelText?: string
    acceptText?: string
}

/**
 * Modal-specific props for creating a modal
 */
export interface ModalProps extends UIScreenProps {
    // required for PanelProps to not satisfy ModalProps
    type: "modal"
    allowClickAway?: boolean
}

/**
 * Panel-specific props for creating a panel
 */
export interface PanelProps extends UIScreenProps {
    type: "panel"
    position: PanelPosition
}

/**
 * UIScreen type
 */
export interface UIScreen<T> {
    id: string
    parent: UIScreen<unknown>
    content: ReactElement
    props: ModalProps | PanelProps
    onClose: UICallback<[CloseType], void>
    onCancel: UICallback<[], void>
    onAccept: UICallback<[T], void>
    onBeforeAccept: UICallback<[], T>
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

export interface Modal<T> extends UIScreen<T> {
    props: ModalProps
}

export interface Panel<T> extends UIScreen<T> {
    props: PanelProps
}

export type OpenModalFn = <T>(contents: ReactElement, parent?: UIScreen<T>, props?: Omit<ModalProps, "type">) => string
export type OpenPanelFn = <T>(contents: ReactElement, parent?: UIScreen<T>, props?: Omit<PanelProps, "type">) => string
export type CloseModalFn = (closeType: CloseType) => void
export type ClosePanelFn = (id: string, closeType: CloseType) => void
export type AddToastFn = (variant: VariantType, ...contents: string[]) => void
export type ConfigureScreenFn = <T extends UIScreen<any>>(
    screen: T,
    props: T extends Panel<infer _> ? Partial<PanelProps> : Partial<ModalProps>,
    callbacks: T extends Modal<infer S>
        ? Omit<Partial<UIScreenCallbacks<S>>, "onBeforeAccept">
        : T extends Panel<infer S>
          ? Omit<Partial<UIScreenCallbacks<S>>, "onBeforeAccept">
          : never
) => void

export type UIContextProps = {
    modal?: Modal<unknown>
    panels: Panel<unknown>[]
    openModal: OpenModalFn
    openPanel: OpenPanelFn
    closeModal: CloseModalFn
    closePanel: ClosePanelFn
    addToast: AddToastFn
    configureScreen: ConfigureScreenFn
}

export const UIContext = createContext<UIContextProps>({
    panels: [],
    openModal: (_content, _parent, _props = { hideAccept: false, hideCancel: false }) => "",
    openPanel: (_content, _parent, _props = { hideAccept: false, hideCancel: false, position: "center" }) => "",
    closeModal: () => {},
    closePanel: _id => {},
    addToast: (_variant, _msg) => "",
    configureScreen: (_screen, _props) => {},
})

export const useUIContext = () => useContext(UIContext)

export const UIProvider: React.FC<UIProviderProps> = ({ children }) => {
    const [modal, setModal] = useState<Modal<unknown> | undefined>(undefined)
    const [panels, setPanels] = useState<Panel<unknown>[]>([])

    const { enqueueSnackbar } = useSnackbar()

    const openModal: OpenModalFn = useCallback(
        <T,>(
            content: ReactElement,
            parent?: UIScreen<T>,
            props: Omit<ModalProps, "type"> & Omit<UIScreenCallbacks<T>, "onAccept"> = {
                hideAccept: false,
                hideCancel: false,
                acceptText: "Accept",
                cancelText: "Cancel",
            }
        ) => {
            const id = uuidv4()
            const newModal = {
                id,
                parent,
                content,
                props,
            } as Modal<T>
            modal?.onClose?.(CloseType.Overwrite)

            newModal.onClose = new UICallback()
            if (props.onClose) newModal.onClose.setUserDefinedFunc(props.onClose)

            newModal.onBeforeAccept = new UICallback()
            if (props.onBeforeAccept) newModal.onBeforeAccept.setUserDefinedFunc(props.onBeforeAccept)

            newModal.onCancel = new UICallback()
            if (props.onCancel) newModal.onCancel.setUserDefinedFunc(props.onCancel)

            setModal(newModal as Modal<unknown>)
            return id
        },
        [modal]
    )

    const openPanel: OpenPanelFn = useCallback(
        <T,>(
            content: ReactElement,
            parent?: UIScreen<T>,
            props: Omit<PanelProps, "type"> & Omit<UIScreenCallbacks<T>, "onAccept"> = {
                hideAccept: false,
                hideCancel: false,
                acceptText: "Accept",
                cancelText: "Cancel",
                position: "center",
            }
        ) => {
            const id = uuidv4()
            const panel = {
                id,
                parent,
                content,
                props,
            } as Panel<T>

            panel.onClose = new UICallback()
            if (props.onClose) panel.onClose.setUserDefinedFunc(props.onClose)

            panel.onBeforeAccept = new UICallback()
            if (props.onBeforeAccept) panel.onBeforeAccept.setUserDefinedFunc(props.onBeforeAccept)

            panel.onCancel = new UICallback()
            if (props.onCancel) panel.onCancel.setUserDefinedFunc(props.onCancel)

            setPanels([...panels, panel as Panel<unknown>])
            return id
        },
        [panels]
    )

    const closeCallbacks = <T,>(elem: Panel<T> | Modal<T>, closeType: CloseType) => {
        elem.onClose?.(closeType)
        switch (closeType) {
            case CloseType.Accept: {
                const beforeAcceptResult = elem.onBeforeAccept?.()
                elem.onAccept?.(beforeAcceptResult)
                break
            }
            case CloseType.Cancel:
                elem.onCancel?.()
                break
            default:
                break
        }
    }

    const closeModal = useCallback(
        <T,>(closeType: CloseType) => {
            if (modal) closeCallbacks<T>(modal as Modal<T>, closeType)
            setModal(undefined)
        },
        [modal]
    )

    const closePanel = useCallback((id: string, closeType: CloseType) => {
        setPanels(p => {
            const panel = p.find((p: Panel<unknown>) => p.id === id)
            if (panel) closeCallbacks(panel, closeType)
            return p.filter((pnl: Panel<unknown>) => pnl.id !== id)
        })
    }, [])

    const addToast = useCallback(
        (variant: VariantType, ...contents: string[]) => {
            enqueueSnackbar(contents.join("\n"), { variant })
        },
        [enqueueSnackbar]
    )

    const configureScreen: ConfigureScreenFn = (screen, props, callbacks) => {
        type PropKey = keyof typeof screen.props
        type PropValue = (typeof screen.props)[keyof typeof screen.props]

        for (const [k, v] of Object.entries(props)) {
            ;(screen.props as Record<PropKey, PropValue>)[k as PropKey] = v as PropValue
        }

        if (callbacks.onAccept) screen.onAccept.setDefaultFunc(callbacks.onAccept)
        if (callbacks.onCancel) screen.onCancel.setDefaultFunc(callbacks.onCancel)
        if (callbacks.onClose) screen.onClose.setDefaultFunc(callbacks.onClose)
    }

    const m = {} as Modal<number>
    configureScreen(m, {} as ModalProps, {} as UIScreenCallbacks<number>)

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
                configureScreen,
            }}
        >
            {children}
        </UIContext.Provider>
    )
}
