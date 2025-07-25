import { VariantType } from "notistack"
import { createContext, ReactElement, ReactNode, useContext } from "react"
import { UICallback } from "./UICallbacks"

export enum CloseType {
    Accept = 0,
    Cancel = 1,
    Overwrite = 2,
}

export interface UIScreenCallbacks<T> {
    onClose?: () => void
    onCancel?: () => void
    onBeforeAccept?: () => T
    onAccept?: (arg: T) => void
}

/**
 *  Props for generic UIScreen
 */
export interface UIScreenProps {
    configured: boolean
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

export type OpenModalFn = <T>(
    contents: ReactElement,
    parent?: UIScreen<T>,
    props?: Omit<ModalProps, "type" | "configured">
) => string
export type OpenPanelFn = <T>(
    contents: ReactElement,
    parent?: UIScreen<T>,
    props?: Omit<PanelProps, "type" | "configured">
) => string
export type CloseModalFn = (closeType: CloseType) => void
export type ClosePanelFn = (id: string, closeType: CloseType) => void
export type AddToastFn = (variant: VariantType, ...contents: ReactNode[]) => void
// biome-ignore lint/suspicious/noExplicitAny: T necessarily must extend any type of UIScreen
export type ConfigureScreenFn = <T extends UIScreen<any>>(
    screen: T,
    props: T extends Panel<infer _> ? Partial<Omit<PanelProps, "configured">> : Partial<Omit<ModalProps, "configured">>,
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
    addToast: (_variant, ..._msg) => "",
    configureScreen: (_screen, _props) => {},
})

export const useUIContext = () => useContext(UIContext)
