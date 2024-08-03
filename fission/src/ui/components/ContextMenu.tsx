import { Box, Grid } from "@mui/material"
import { useEffect, useRef, useState } from "react"
import { ContextData, ContextSupplierEvent } from "./ContextMenuData"
import { colorNameToVar } from "../ThemeContext"
import Button, { ButtonSize } from "./Button"
import Label, { LabelSize } from "./Label"
import { SectionDivider } from "./StyledComponents"

interface ContextMenuStateData {
    data: ContextData
    location: [number, number]
}

function ContextMenu() {

    const [state, setState] = useState<ContextMenuStateData | undefined>(undefined)

    // const { currentTheme, themes } = useTheme()
    // const theme = useMemo(() => themes[currentTheme], [currentTheme, themes])

    useEffect(() => {
        const func = (e: ContextSupplierEvent) => {
            setState({ data: e.data, location: [e.mousePosition[0], e.mousePosition[1]] })
        }

        ContextSupplierEvent.Listen(func)
        return () => { ContextSupplierEvent.RemoveListener(func) }
    }, [])

    return (!state ? <></> :
        <Box
            id="CANCEL"
            component="div"
            display="flex"
            sx={{
                position: "fixed",
                left: "0pt",
                top: "0pt",
                width: "100vw",
                height: "100vh",
            }}
            onPointerDown={() =>  setState(undefined)}
            onContextMenu={(e) => e.preventDefault()}
        >
            <Box
                id="MENU"
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
                    backgroundColor: colorNameToVar("Background"),
                    color: colorNameToVar("InteractiveElementText")
                }}
                // Why, why, why do I need to do this. This is absurd
                onPointerDown={e => e.stopPropagation()}
            >
                <Box
                    component="div"
                    display="flex"
                    sx={{
                        flexDirection: "column",
                    }}
                >
                    <Label key={"context-title"} size={LabelSize.Small}>{state.data.title}</Label>
                    <SectionDivider />
                </Box>
                {
                    state.data.items.map(x => (<Button
                        size={ButtonSize.Small}
                        className={"w-full text-sm"}
                        value={x.name}
                        onClick={() => {
                            setState(undefined)
                            x.func()
                        }}
                    />))
                }
            </Box>
        </Box>
    )
}

export default ContextMenu