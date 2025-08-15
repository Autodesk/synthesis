import CloseIcon from "@mui/icons-material/Close"
import { IconButton } from "./components/StyledComponents"
import type { SnackbarKey, SnackbarMessage, VariantType } from "notistack"
import { useSnackbar } from "notistack"
import type React from "react"
import type { FunctionComponent, ReactNode } from "react"
import { useCallback, useReducer, useState } from "react"
import { v4 as uuidv4 } from "uuid"
import type { ModalImplProps } from "./components/Modal"
import type { PanelImplProps } from "./components/Panel"
import {
    CloseType,
    type ConfigureScreenFn,
    type Modal,
    type ModalProps,
    type OpenModalFn,
    type OpenPanelFn,
    type Panel,
    type PanelProps,
    UIContext,
    type UIScreen,
    type UIScreenCallbacks,
    type UIScreenProps,
} from "./helpers/UIProviderHelpers"
import { UICallback } from "./UICallbacks"

export type UIProviderProps = {
    children?: ReactNode
}

// biome-ignore-start lint/suspicious/noExplicitAny: need to be able to extend
export const UIProvider: React.FC<UIProviderProps> = ({ children }) => {
    const [modal, setModal] = useState<Modal<any, any> | undefined>(undefined)
    const [panels, setPanels] = useState<Panel<any, any>[]>([])
    const [_, refresh] = useReducer(x => !x, false)

    const { enqueueSnackbar, closeSnackbar } = useSnackbar()

    const DEFAULT_PROPS = {
        hideAccept: false,
        hideCancel: false,
        acceptText: "Accept",
        cancelText: "Cancel",
    } as UIScreenProps<any>

    const DEFAULT_MODAL_PROPS = {
        ...DEFAULT_PROPS,
        allowClickAway: true,
    }

    const DEFAULT_PANEL_PROPS = {
        ...DEFAULT_PROPS,
        position: "right",
    } as PanelProps<any>

    const openModal: OpenModalFn = useCallback(
        <T, P>(
            content: FunctionComponent<ModalImplProps<T, P>>,
            customProps: P,
            parent?: UIScreen<any, any>,
            props: Omit<ModalProps<P>, "type" | "configured" | "custom"> &
                Omit<UIScreenCallbacks<T>, "onBeforeAccept"> = DEFAULT_PROPS
        ) => {
            const id = uuidv4()
            const newModal = {
                id,
                parent,
                content,
                props: {
                    ...DEFAULT_MODAL_PROPS,
                    ...props,
                    custom: customProps,
                },
            } as Modal<T, P>
            modal?.onClose?.(CloseType.Overwrite)

            newModal.props.configured = false

            // don't allow configuring onAccept from open function
            newModal.onAccept = new UICallback()

            newModal.onClose = new UICallback()
            if (props.onClose) newModal.onClose.setUserDefinedFunc(props.onClose)

            newModal.onAccept = new UICallback()
            if (props.onAccept) newModal.onAccept.setUserDefinedFunc(props.onAccept)

            newModal.onCancel = new UICallback()
            if (props.onCancel) newModal.onCancel.setUserDefinedFunc(props.onCancel)

            setModal(newModal as Modal<any, any>)
            return id
        },
        [modal]
    )

    const openPanel: OpenPanelFn = useCallback(
        <T, P>(
            content: FunctionComponent<PanelImplProps<T, P>>,
            customProps: P,
            parent?: UIScreen<any, any>,
            props: Omit<PanelProps<P>, "type" | "configured" | "custom"> &
                Omit<UIScreenCallbacks<T>, "onBeforeAccept"> = DEFAULT_PANEL_PROPS
        ) => {
            const id = uuidv4()
            const panel = {
                id,
                parent,
                content,
                props: {
                    ...DEFAULT_PANEL_PROPS,
                    ...props,
                    custom: customProps,
                },
            } as Panel<T, P>

            panel.props.configured = false

            // don't allow configuring onAccept from open function
            panel.onAccept = new UICallback()

            panel.onClose = new UICallback()
            if (props.onClose) panel.onClose.setUserDefinedFunc(props.onClose)

            panel.onAccept = new UICallback()
            if (props.onAccept) panel.onAccept.setUserDefinedFunc(props.onAccept)

            panel.onCancel = new UICallback()
            if (props.onCancel) panel.onCancel.setUserDefinedFunc(props.onCancel)

            setPanels([...panels, panel as Panel<any, any>])
            return id
        },
        [panels]
    )

    const closeCallbacks = <T, P>(elem: Panel<T, P> | Modal<T, P>, closeType: CloseType) => {
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
        <T, P>(closeType: CloseType) => {
            if (modal) closeCallbacks<T, P>(modal as Modal<T, P>, closeType)
            setModal(undefined)
        },
        [modal]
    )

    const closePanel = useCallback((id: string, closeType: CloseType) => {
        setPanels(p => {
            const panel = p.find((p: Panel<any, any>) => p.id === id)
            if (panel) closeCallbacks(panel, closeType)
            return p.filter((pnl: Panel<any, any>) => pnl.id !== id)
        })
    }, [])
    // biome-ignore-end lint/suspicious/noExplicitAny: need to be able to extend

    const snackbarAction = useCallback(
        (snackbarId: SnackbarKey) => (
            <IconButton onClick={() => closeSnackbar(snackbarId)}>
                <CloseIcon />
            </IconButton>
        ),
        []
    )

    const addToast = useCallback(
        (variant: VariantType, ...contents: SnackbarMessage[]) => {
            enqueueSnackbar(
                contents.length <= 1 ? (
                    <>{...contents}</>
                ) : (
                    <>
                        {...contents.map(child => (
                            <>
                                {child}
                                <br />
                            </>
                        ))}
                    </>
                ),
                { variant, action: snackbarAction }
            )
        },
        [enqueueSnackbar]
    )

    const configureScreen: ConfigureScreenFn = useCallback((screen, props, callbacks) => {
        type PropKey = keyof typeof screen.props
        type PropValue = (typeof screen.props)[keyof typeof screen.props]

        for (const [k, v] of Object.entries(props)) {
            ;(screen.props as Record<PropKey, PropValue>)[k as PropKey] = v as PropValue
        }

        screen.props.configured = true

        if (callbacks.onBeforeAccept) screen.onAccept.setDefaultFunc(callbacks.onBeforeAccept)
        if (callbacks.onCancel) screen.onCancel.setDefaultFunc(callbacks.onCancel)
        if (callbacks.onClose) screen.onClose.setDefaultFunc(callbacks.onClose)

        refresh()
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
                configureScreen,
            }}
        >
            {children}
        </UIContext.Provider>
    )
}
