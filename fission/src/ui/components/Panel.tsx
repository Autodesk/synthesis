import { Button, Card, CardActions, CardContent, CardHeader } from "@mui/material"
import React, { type ReactElement, useRef } from "react"
import Draggable from "react-draggable"
import {
    CloseType,
    type Modal as ModalType,
    type PanelPosition,
    type Panel as PanelType,
    useUIContext,
} from "../helpers/UIProviderHelpers"

// biome-ignore-start lint/suspicious/noExplicitAny: need to be able to extend
export type PanelImplProps<T, P> = Partial<{
    panel: PanelType<T, P>
    parent?: PanelType<any, any> | ModalType<any, any>
}>

interface PanelElementProps<T, P> {
    children?: ReactElement<PanelImplProps<any, any>>
    panel: PanelType<T, P>
    parent?: PanelType<any, any> | ModalType<any, any>
}
// biome-ignore-end lint/suspicious/noExplicitAny: need to be able to extend

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

export const Panel = <T, P>({ children, panel, parent }: PanelElementProps<T, P>) => {
    const { closePanel } = useUIContext()

    const props = panel.props
    const nodeRef = useRef<HTMLDivElement | null>(null)

    // FIXME: sliders show up as <span> so want to cancel drag on those
    // however still can drag on dropdown but menu elements are left behind
    return (
        <Draggable
            handle=".panel-drag-handle"
            cancel={"input, textarea, select, .MuiSlider-root, .MuiMenuItem-root, .no-drag"}
            positionOffset={getPositionOffset(props.position)}
            nodeRef={nodeRef}
        >
            <Card
                elevation={8}
                sx={{
                    display: panel.props.configured ? "" : "none",
                    position: "absolute",
                    pointerEvents: "auto",
                    p: 2,
                    boxShadow: 6,
                    maxHeight: "70vh",
                    overflowY: "auto",
                }}
                ref={nodeRef}
            >
                {props.title && (
                    <CardHeader
                        title={props.title}
                        className="panel-drag-handle select-none"
                        sx={{
                            cursor: "move",
                            py: 1,
                            px: 2,
                            borderBottom: theme => `1px solid ${theme.palette.divider}`,
                        }}
                        titleTypographyProps={{ variant: "subtitle1" }}
                        action={
                            <div
                                className="h-2 w-10 rounded-sm"
                                style={{
                                    background:
                                        "linear-gradient(90deg, rgba(255,255,255,0.2) 25%, rgba(0,0,0,0.2) 25%, rgba(0,0,0,0.2) 50%, rgba(255,255,255,0.2) 50%, rgba(255,255,255,0.2) 75%, rgba(0,0,0,0.2) 75%)",
                                    backgroundSize: "8px 100%",
                                }}
                            />
                        }
                    />
                )}
                <CardContent sx={{ p: 2, "&:last-child": { pb: 2 } }}>
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
                                disabled={props.disableAccept}
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
