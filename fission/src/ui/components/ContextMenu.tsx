import { Box } from "@mui/material"
import { useEffect, useState } from "react"
import { ContextData, ContextSupplierEvent } from "./ContextMenuData"
import { colorNameToVar } from "../ThemeContext"

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
            setState({ data: e.data, location: [e.mouseEvent.clientX, e.mouseEvent.clientY] })
        }

        ContextSupplierEvent.Listen(func)
        return () => { ContextSupplierEvent.RemoveListener(func) }
    }, [])

    return (!state ? <></> :
        <Box
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
            onContextMenu={(e) => e.preventDefault()}
        >
            <Box
                component="div"
                display="flex"
                sx={{
                    position: "fixed",
                    left: state.location[0],
                    top: state.location[1],
                    padding: "1rem",
                    backgroundColor: colorNameToVar("Background"),
                    color: colorNameToVar("InteractiveElementText")
                }}
            >
                {state.data.title}
            </Box>
        </Box>
    )
}

export default ContextMenu