import { Divider, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { type ContextData, ContextSupplierEvent } from "./ContextMenuData"
import { globalOpenModal, globalOpenPanel } from "./GlobalUIControls"
import Label from "./Label"
import { Button } from "./StyledComponents"

interface ContextMenuStateData {
    data: ContextData
    location: [number, number]
}

const ContextMenu: React.FC = () => {
    const [state, setState] = useState<ContextMenuStateData | undefined>(undefined)

    useEffect(() => {
        const func = (e: ContextSupplierEvent) => {
            setState({
                data: e.data,
                location: [e.mousePosition[0], e.mousePosition[1]],
            })
        }

        ContextSupplierEvent.listen(func)
        return () => {
            ContextSupplierEvent.removeListener(func)
        }
    }, [])

    return !state ? (
        <></>
    ) : (
        <Stack
            key="CANCEL"
            component="div"
            direction="column"
            sx={{
                position: "fixed",
                left: "0pt",
                top: "0pt",
                width: "100vw",
                height: "100vh",
            }}
            onPointerDown={() => setState(undefined)}
            onContextMenu={e => e.preventDefault()}
        >
            <Stack
                key="MENU"
                component="div"
                direction="column"
                sx={{
                    gap: "0.5rem",
                    position: "fixed",
                    left: state.location[0],
                    top: state.location[1],
                    padding: "1rem",
                    borderRadius: "0.5rem",
                    bgcolor: "background.paper",
                    boxShadow: 6,
                }}
                // Why, why, why do I need to do this. This is absurd
                onPointerDown={e => e.stopPropagation()}
            >
                <Stack key="CONTEXT-HEADER" component="div" direction="column">
                    <Label key="context-title" size="md" color="text.primary">
                        {state.data.title}
                    </Label>
                    <Divider />
                </Stack>
                {state.data.items.map(x => (
                    <Button
                        key={x.name}
                        className={"w-full text-sm"}
                        onClick={() => {
                            setState(undefined)
                            if (x.screen) {
                                if (x.type === "modal") {
                                    globalOpenModal(x.screen, x.customProps)
                                } else {
                                    globalOpenPanel(x.screen, x.customProps)
                                }
                            }
                            x.func?.()
                        }}
                    >
                        {x.name}
                    </Button>
                ))}
            </Stack>
        </Stack>
    )
}

export default ContextMenu
