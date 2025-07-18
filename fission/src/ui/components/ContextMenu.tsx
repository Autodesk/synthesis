import { Box, Button, Divider, Typography } from "@mui/material"
import { useEffect, useState } from "react"
import { type ContextData, ContextSupplierEvent } from "./ContextMenuData"
// import { colorNameToVar } from "../ThemeContext"

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
        <Box
            key="CANCEL"
            component="div"
            display="flex"
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
            <Box
                key="MENU"
                component="div"
                display="flex"
                sx={{
                    gap: "0.5rem",
                    flexDirection: "column",
                    position: "fixed",
                    left: state.location[0],
                    top: state.location[1],
                    padding: "1rem",
                    borderRadius: "0.5rem",
                    // backgroundColor: colorNameToVar("Background"),
                    // color: colorNameToVar("InteractiveElementText"),
                }}
                // Why, why, why do I need to do this. This is absurd
                onPointerDown={e => e.stopPropagation()}
            >
                <Box
                    key="CONTEXT-HEADER"
                    component="div"
                    display="flex"
                    sx={{
                        flexDirection: "column",
                    }}
                >
                    <Typography key="context-title">{state.data.title}</Typography>
                    <Divider />
                </Box>
                {state.data.items.map(x => (
                    <Button
                        key={x.name}
                        className={"w-full text-sm"}
                        onClick={() => {
                            setState(undefined)
                            x.func()
                        }}
                    >
                        {x.name}
                    </Button>
                ))}
            </Box>
        </Box>
    )
}

export default ContextMenu
