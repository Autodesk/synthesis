import CloseIcon from "@mui/icons-material/Close"
import type { SnackbarKey, SnackbarMessage, VariantType } from "notistack"
import { useSnackbar } from "notistack"
import type React from "react"
import { useMemo } from "react"
import type { FunctionComponent, ReactNode } from "react"
import { Fragment, useCallback, useReducer, useState } from "react"
import { v4 as uuidv4 } from "uuid"
import type { ModalImplProps } from "./components/Modal"
import type { PanelImplProps } from "./components/Panel"
import { IconButton } from "./components/StyledComponents"
import {
    CloseType,
    type ConfigureScreenFn,
    type Modal,
    type ModalProps,
    type OpenModalFn,
    type OpenPanelFn,
    type Panel,
    type PanelProps,
    type TogglePanelFn,
    type UIBlockState,
    UIContext,
    type UIScreen,
    type UIScreenCallbacks,
    type UIScreenProps,
} from "./helpers/UIProviderHelpers"
import { UICallback } from "./UICallbacks"
import InputSystem from "@/systems/input/InputSystem.ts"

export type UIProviderProps = {
    children?: ReactNode
}

const isPlainObject = (x: unknown): x is Record<string, unknown> =>
    typeof x === "object" && x !== null && !Array.isArray(x)

function shallowEqualProps(a: unknown, b: unknown): boolean {
    if (a === b) return true
    if (!isPlainObject(a) || !isPlainObject(b)) return false
    const aKeys = Object.keys(a)
    const bKeys = Object.keys(b)
    if (aKeys.length !== bKeys.length) return false
    return aKeys.every(k => a[k] === b[k])
}

// biome-ignore-start lint/suspicious/noExplicitAny: need to be able to extend
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

const closeCallbacks = <T, P>(elem: Panel<T, P> | Modal<T, P>, closeType: CloseType) => {
    elem.onClose?.(closeType)
    switch (closeType) {
        case CloseType.ACCEPT: {
            const beforeAcceptResult = elem.onBeforeAccept?.()
            elem.onAccept?.(beforeAcceptResult)
            break
        }
        case CloseType.CANCEL:
            elem.onCancel?.()
            break
        default:
            break
    }
}

export const UIProvider: React.FC<UIProviderProps> = ({ children }) => {
    const [modal, setModal] = useState<Modal<any, any> | undefined>(undefined)
    const [panels, setPanels] = useState<Panel<any, any>[]>([])

    const [refreshDep, refresh] = useReducer(x => !x, false)

    const { enqueueSnackbar, closeSnackbar } = useSnackbar()

    InputSystem.escapeKeyListeners[1] = () => {
        if (modal != null) {
            if (!modal.props.hideCancel) {
                closeModal(CloseType.CANCEL)
            }
            return true
        }
        return false
    }

    InputSystem.escapeKeyListeners[2] = () => {
        if (panels.length > 0) {
            const panel = panels[panels.length - 1]
            if (!panel.props.hideCancel) {
                closePanel(panel.id, CloseType.CANCEL)
                return true
            }
        }
        return false
    }

    const blockState: UIBlockState = useMemo(() => {
        const blockingPanel = panels.find(p => p.props.blocking)
        if (blockingPanel != null) {
            return {
                blocked: true,
                blockMessage:
                    blockingPanel?.props?.blockingMessage ?? "Close the current panel before opening another.",
            }
        }
        return {
            blocked: false,
        }
    }, [refreshDep, panels])

    const openModal: OpenModalFn = useCallback(
        <T, P>(
            content: FunctionComponent<ModalImplProps<T, P>>,
            customProps: P,
            parent?: UIScreen<any, any>,
            props: Omit<ModalProps<P>, "type" | "configured" | "custom"> &
                Omit<UIScreenCallbacks<T>, "onBeforeAccept"> = DEFAULT_PROPS
        ) => {
            if (blockState.blocked) {
                addToast("warning", blockState.blockMessage)
                return null
            }

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
            modal?.onClose?.(CloseType.OVERWRITE)

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
        [modal, blockState]
    )

    const snackbarAction = useCallback(
        (snackbarId: SnackbarKey) => (
            <IconButton onClick={() => closeSnackbar(snackbarId)}>
                <CloseIcon />
            </IconButton>
        ),
        [closeSnackbar]
    )

    const addToast = useCallback(
        (variant: VariantType, ...contents: SnackbarMessage[]) => {
            enqueueSnackbar(
                contents.length <= 1 ? (
                    (contents[0] ?? "")
                ) : (
                    <span>
                        {contents.map((content, index) => (
                            <Fragment key={index}>
                                {content}
                                {index < contents.length - 1 && <br />}
                            </Fragment>
                        ))}
                    </span>
                ),
                { variant, action: snackbarAction }
            )
        },
        [enqueueSnackbar, snackbarAction]
    )

    const openPanel: OpenPanelFn = useCallback(
        <T, P>(
            content: FunctionComponent<PanelImplProps<T, P>>,
            customProps: P,
            parent?: UIScreen<any, any>,
            props: Omit<PanelProps<P>, "type" | "configured" | "custom"> &
                Omit<UIScreenCallbacks<T>, "onBeforeAccept"> = DEFAULT_PANEL_PROPS
        ) => {
            // Dupe check
            const existingDuplicate = panels.find(p => p.content === content)
            if (existingDuplicate) {
                const existingCustom = (existingDuplicate.props as PanelProps<P>).custom
                if (customProps === undefined || shallowEqualProps(customProps, existingCustom)) {
                    setPanels(p => [...p.filter(x => x !== existingDuplicate), existingDuplicate])
                    return existingDuplicate.id
                }
            }

            // if any (generic) open panel declares itself as blocking, prevent opening a new one
            if (blockState.blocked) {
                addToast("warning", blockState.blockMessage)
                return null
            }

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

            const exclusiveGroup = panel.props.exclusiveGroup
            if (exclusiveGroup != null) {
                const existing = panels.find(p => p.props.exclusiveGroup == exclusiveGroup)
                if (existing) {
                    closePanel(existing.id, CloseType.OVERWRITE)
                }
            }

            setPanels(panels => {
                const nextPanels = existingDuplicate ? panels.filter(p => p !== existingDuplicate) : panels
                return [...nextPanels, panel as Panel<any, any>]
            })
            return id
        },
        [panels, addToast]
    )

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

    const togglePanel: TogglePanelFn = useCallback(
        <T, P>(
            content: FunctionComponent<PanelImplProps<T, P>>,
            customProps: P,
            matchesOpen?: (openCustomProps: P) => boolean
        ) => {
            const openInstance = panels.find(p => p.content === content)
            if (openInstance && (matchesOpen?.((openInstance.props as PanelProps<P>).custom) ?? true)) {
                // OVERWRITE, not CANCEL: dismissing a panel by re-clicking its icon is not a
                // rejection of the user's edits, so panels get to save them via onClose.
                closePanel(openInstance.id, CloseType.OVERWRITE)
                return null
            }
            return openPanel(content, customProps)
        },
        [panels, openPanel, closePanel]
    )
    // biome-ignore-end lint/suspicious/noExplicitAny: need to be able to extend

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
                blockState,
                openModal,
                openPanel,
                togglePanel,
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
