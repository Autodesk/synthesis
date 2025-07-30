import { IconButton } from "@mui/material"
import CloseIcon from '@mui/icons-material/Close'
import type { SnackbarKey, SnackbarMessage, VariantType } from "notistack"
import { useSnackbar } from "notistack"
import type React from "react"
import type { ReactElement, ReactNode } from "react"
import { useCallback, useState } from "react"
import { v4 as uuidv4 } from "uuid"
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

export const UIProvider: React.FC<UIProviderProps> = ({ children }) => {
    const [modal, setModal] = useState<Modal<unknown> | undefined>(undefined)
    const [panels, setPanels] = useState<Panel<unknown>[]>([])

    const { enqueueSnackbar, closeSnackbar } = useSnackbar()

    const DEFAULT_PROPS = {
        hideAccept: false,
        hideCancel: false,
        acceptText: "Accept",
        cancelText: "Cancel",
    } as UIScreenProps

    const DEFAULT_PANEL_PROPS = {
        ...DEFAULT_PROPS,
        position: "center",
    } as PanelProps

    const openModal: OpenModalFn = useCallback(
        <T,>(
            content: ReactElement,
            parent?: UIScreen<T>,
            props: Omit<ModalProps, "type" | "configured"> &
                Omit<UIScreenCallbacks<T>, "onBeforeAccept"> = DEFAULT_PROPS
        ) => {
            const id = uuidv4()
            const newModal = {
                id,
                parent,
                content,
                props: {
                    ...DEFAULT_PROPS,
                    ...props,
                },
            } as Modal<T>
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

            setModal(newModal as Modal<unknown>)
            return id
        },
        [modal]
    )

    const openPanel: OpenPanelFn = useCallback(
        <T,>(
            content: ReactElement,
            parent?: UIScreen<T>,
            props: Omit<PanelProps, "type" | "configured"> &
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
                },
            } as Panel<T>

            panel.props.configured = false

            // don't allow configuring onAccept from open function
            panel.onAccept = new UICallback()

            panel.onClose = new UICallback()
            if (props.onClose) panel.onClose.setUserDefinedFunc(props.onClose)

            panel.onAccept = new UICallback()
            if (props.onAccept) panel.onAccept.setUserDefinedFunc(props.onAccept)

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

    const snackbarAction = useCallback((snackbarId: SnackbarKey) => (
        <IconButton onClick={() => closeSnackbar(snackbarId)}>
            <CloseIcon />
        </IconButton>
    ), [])

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

    const configureScreen: ConfigureScreenFn = (screen, props, callbacks) => {
        type PropKey = keyof typeof screen.props
        type PropValue = (typeof screen.props)[keyof typeof screen.props]

        for (const [k, v] of Object.entries(props)) {
            ;(screen.props as Record<PropKey, PropValue>)[k as PropKey] = v as PropValue
        }

        screen.props.configured = true

        if (callbacks.onBeforeAccept) screen.onAccept.setDefaultFunc(callbacks.onBeforeAccept)
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
