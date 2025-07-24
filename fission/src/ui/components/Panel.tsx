import { Button, Card, CardActions, CardContent, CardHeader } from "@mui/material"
import React, { useEffect, useMemo, useReducer, useState, type ReactElement } from "react"
import Draggable from "react-draggable"
import {
    CloseType,
    type Modal as ModalType,
    type PanelPosition,
    type Panel as PanelType,
    useUIContext,
} from "../UIProvider"

export type PanelImplProps<T> = Partial<{
    panel: PanelType<T>
    parent: PanelType<T> | ModalType<T>
}>

interface PanelElementProps<T> {
    children?: ReactElement<PanelImplProps<T>>
    panel: PanelType<T>
    parent?: PanelType<T> | ModalType<T>
}

// TODO: I don't like this
const HALF_W = "calc(50vw - 50%)"
const HALF_H = "calc(50vh - 50%)"
const FULL_W = "calc(100vw - 100%)"
const FULL_H = "calc(100vh - 100%)"

// TODO: optimize?
const getPositionOffset = (position: PanelPosition) => {
    switch (position) {
        case "top-left":
            return { x: 0, y: 0 }
        case "top":
            return { x: HALF_W, y: 0 }
        case "top-right":
            return { x: FULL_W, y: 0 }
        case "left":
            return { x: 0, y: HALF_H }
        case "right":
            return { x: FULL_W, y: HALF_H }
        case "bottom-left":
            return { x: 0, y: FULL_H }
        case "bottom":
            return { x: HALF_W, y: FULL_H }
        case "bottom-right":
            return { x: FULL_W, y: FULL_H }
        default:
            return { x: HALF_W, y: HALF_H }
    }
}

export const Panel = <T,>({ children, panel, parent }: PanelElementProps<T>) => {
    const { closePanel } = useUIContext()

    const [_, refresh] = useState(false)

    const props = panel.props

    // biome-ignore lint/correctness/useExhaustiveDependencies: to refresh on configure
    useEffect(() => {
        refresh(x => !x)
    }, [panel.props.title])

    // FIXME: sliders show up as <span> so want to cancel drag on those
    // however still can drag on dropdown but menu elements are left behind
    return (
        <Draggable cancel="span" positionOffset={getPositionOffset(props.position)}>
            <Card
                sx={{
                    display: panel.props.title ? "" : "none",
                    position: "absolute",
                    maxWidth: 400,
                    pointerEvents: "auto",
                    p: 4,
                }}
            >
                {props.title && <CardHeader title={props.title} />}
                <CardContent>
                    <div className="panel-contents">
                        {React.Children.map(children, child => {
                            if (React.isValidElement(child)) return React.cloneElement(child, { panel, parent })
                        })}
                    </div>
                </CardContent>
                {(!props.hideCancel || !props.hideAccept) && (
                    <CardActions>
                        {!props.hideCancel && (
                            <Button
                                onClick={() => closePanel(panel.id, CloseType.Cancel)}
                                variant="outlined"
                                color="secondary"
                            >
                                {props.cancelText ?? "Cancel"}
                            </Button>
                        )}
                        {!props.hideAccept && (
                            <Button
                                onClick={() => closePanel(panel.id, CloseType.Accept)}
                                variant="contained"
                                color="primary"
                            >
                                {props.acceptText ?? "Accept"}
                            </Button>
                        )}
                    </CardActions>
                )}
            </Card>
        </Draggable>
    )
}
