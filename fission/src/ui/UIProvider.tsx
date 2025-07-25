import type { SnackbarMessage, VariantType } from "notistack"
import { useSnackbar } from "notistack"
import type React from "react"
import type { ReactElement, ReactNode } from "react"
import { useCallback, useState } from "react"
import { v4 as uuidv4 } from "uuid"
import { UICallback } from "./UICallbacks"
import {
    CloseType,
    ConfigureScreenFn, 
    Modal,
    ModalProps,
    OpenModalFn,
    OpenPanelFn,
    Panel,
    PanelProps,
    UIContext,
    UIScreen,
    UIScreenCallbacks
} from "./helpers/UIProviderHelpers"

export type UIProviderProps = {
    children?: ReactNode
}

export const UIProvider: React.FC<UIProviderProps> = ({ children }) => {
    const [modal, setModal] = useState<Modal<unknown> | undefined>(undefined)
    const [panels, setPanels] = useState<Panel<unknown>[]>([])

    const { enqueueSnackbar } = useSnackbar()

    const openModal: OpenModalFn = useCallback(
        <T,>(
            content: ReactElement,
            parent?: UIScreen<T>,
            props: Omit<ModalProps, "type" | "configured"> & Omit<UIScreenCallbacks<T>, "onAccept"> = {
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

            newModal.props.configured = false

            // don't allow configuring onAccept from open function
            newModal.onAccept = new UICallback()

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
            props: Omit<PanelProps, "type" | "configured"> & Omit<UIScreenCallbacks<T>, "onAccept"> = {
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

            panel.props.configured = false

            // don't allow configuring onAccept from open function
            panel.onAccept = new UICallback()

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
                { variant }
            )
        },
        [enqueueSnackbar]
    )

    const configureScreen: ConfigureScreenFn = (screen, props, callbacks) => {
        type PropKey = keyof typeof screen.props
        type PropValue = (typeof screen.props)[keyof typeof screen.props]

        for (const [k, v] of Object.entries(props)) {
            ;(screen.props as Record<PropKey, PropValue>)[k as PropKey] = v as PropValue
        }

        screen.props.configured = true

        if (callbacks.onAccept) screen.onAccept.setDefaultFunc(callbacks.onAccept)
        if (callbacks.onCancel) screen.onCancel.setDefaultFunc(callbacks.onCancel)
        if (callbacks.onClose) screen.onClose.setDefaultFunc(callbacks.onClose)
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
                addToast,
                configureScreen,
            }}
        >
            {children}
        </UIContext.Provider>
    )
}
