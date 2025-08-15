import type { VariantType } from "notistack"
import { createContext, type FunctionComponent, type ReactNode, useContext } from "react"
import type { ModalImplProps } from "../components/Modal"
import type { PanelImplProps } from "../components/Panel"
import type { UICallback } from "../UICallbacks"

export enum CloseType {
    Accept = 0,
    Cancel = 1,
    Overwrite = 2,
}

export interface UIScreenCallbacks<T> {
    onClose?: (closeType: CloseType) => void
    onCancel?: () => void
    onBeforeAccept?: () => T
    onAccept?: (arg: T) => void
}

/**
 *  Props for generic UIScreen
 */
export interface UIScreenProps<P> {
    configured: boolean
    title?: string
    hideCancel?: boolean
    hideAccept?: boolean
    disableAccept?: boolean
    cancelText?: string
    acceptText?: string
    custom: P
}

/**
 * Modal-specific props for creating a modal
 */
export interface ModalProps<P> extends UIScreenProps<P> {
    // required for PanelProps to not satisfy ModalProps
    type: "modal"
    allowClickAway?: boolean
}

/**
 * Panel-specific props for creating a panel
 */
export interface PanelProps<P> extends UIScreenProps<P> {
    type: "panel"
    position: PanelPosition
}

// biome-ignore-start lint/suspicious/noExplicitAny: need to be able to extend

export interface UIScreen<T, P> {
    id: string
    parent?: UIScreen<any, any>
    content: FunctionComponent
    props: ModalProps<P> | PanelProps<P>
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

export interface Modal<T, P> extends UIScreen<T, P> {
    props: ModalProps<P>
}

export interface Panel<T, P> extends UIScreen<T, P> {
    props: PanelProps<P>
}

export type OpenModalFn = <T, P>(
    content: FunctionComponent<ModalImplProps<T, P>>,
    customProps: P,
    parent?: UIScreen<any, any>,
    props?: Omit<ModalProps<P>, "type" | "configured" | "custom"> & Omit<UIScreenCallbacks<T>, "onBeforeAccept">
) => string
export type OpenPanelFn = <T, P>(
    content: FunctionComponent<PanelImplProps<T, P>>,
    customProps: P,
    parent?: UIScreen<any, any>,
    props?: Omit<PanelProps<P>, "type" | "configured" | "custom"> & Omit<UIScreenCallbacks<T>, "onBeforeAccept">
) => string
export type CloseModalFn = (closeType: CloseType) => void
export type ClosePanelFn = (id: string, closeType: CloseType) => void
export type AddToastFn = (variant: VariantType, ...contents: ReactNode[]) => void
export type ConfigureScreenFn = <T extends UIScreen<any, any>>(
    screen: T,
    props: T extends Panel<infer _1, infer P>
        ? Partial<Omit<PanelProps<P>, "configured">>
        : T extends Modal<infer _, infer P>
          ? Partial<Omit<ModalProps<P>, "configured">>
          : never,
    callbacks: T extends Modal<infer S, infer _>
        ? Partial<Omit<UIScreenCallbacks<S>, "onAccept">>
        : T extends Panel<infer S, infer _>
          ? Partial<Omit<UIScreenCallbacks<S>, "onAccept">>
          : never
) => void

export type UIContextProps = {
    modal?: Modal<any, any>
    panels: Panel<any, any>[]
    openModal: OpenModalFn
    openPanel: OpenPanelFn
    closeModal: CloseModalFn
    closePanel: ClosePanelFn
    addToast: AddToastFn
    configureScreen: ConfigureScreenFn
}

// biome-ignore-end lint/suspicious/noExplicitAny: need to be able to extend

export const UIContext = createContext<UIContextProps>({
    panels: [],
    openModal: (_content, _customProps, _parent, _props = { hideAccept: false, hideCancel: false }) => "",
    openPanel: (
        _content,
        _customProps,
        _parent,
        _props = { hideAccept: false, hideCancel: false, position: "center" }
    ) => "",
    closeModal: _closeType => {},
    closePanel: (_id, _closeType) => {},
    addToast: (_variant, ..._msg) => "",
    configureScreen: (_screen, _props) => {},
})

export const useUIContext = () => useContext(UIContext)
