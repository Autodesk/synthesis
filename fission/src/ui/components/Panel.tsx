import { Card, CardActions, CardContent, CardHeader } from "@mui/material"
import React, { type ReactElement, useRef } from "react"
import Draggable from "react-draggable"
import {
    CloseType,
    type Modal as ModalType,
    type PanelPosition,
    type Panel as PanelType,
    useUIContext,
} from "../helpers/UIProviderHelpers"
import { Button } from "./StyledComponents"

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
                    display: panel.props.configured ? "flex" : "none",
                    position: "absolute",
                    pointerEvents: "auto",
                    p: 0,
                    backgroundColor: "#2e2e2e",
                    boxShadow: 6,
                    maxHeight: "85vh",
                    flexDirection: "column",
                }}
                ref={nodeRef}
            >
                {props.title && (
                    <CardHeader
                        title={props.title}
                        className="panel-drag-handle select-none hover:cursor-grab active:cursor-grabbing"
                        sx={{
                            cursor: "move",
                            py: 1,
                            px: 2,
                            borderBottom: theme => `1px solid ${theme.palette.divider}`,
                            position: "sticky",
                            top: 0,
                            zIndex: 1,
                            backgroundColor: "inherit",
                        }}
                        titleTypographyProps={{ variant: "h5" }}
                    />
                )}
                <CardContent
                    sx={{
                        p: 2,
                        flex: "1 1 auto",
                        overflowY: "auto",
                        "&:last-child": { pb: 2 },
                        backgroundColor: "inherit",
                    }}
                >
                    <div className="panel-contents">
                        {React.Children.map(children, child => {
                            if (React.isValidElement(child)) return React.cloneElement(child, { panel, parent })
                        })}
                    </div>
                </CardContent>
                {(!props.hideCancel || !props.hideAccept) && (
                    <CardActions
                        sx={{
                            position: "sticky",
                            bottom: 0,
                            zIndex: 1,
                            bgcolor: "inherit",
                            borderTop: theme => `1px solid ${theme.palette.divider}`,
                            p: 2,
                        }}
                    >
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
