import { Alert, type AlertColor, Snackbar } from "@mui/material"
import { useState } from "react"
import { setGlobalSetAlert } from "../lib/GlobalUtils.tsx"

function GlobalAlert() {
    const [active, setActive] = useState(false)
    const [text, setText] = useState("")
    const [severity, setSeverity] = useState<AlertColor>("info")
    setGlobalSetAlert((severity, text) => {
        setSeverity(severity)
        setText(text)
        setActive(true)
    })
    return (
        <Snackbar
            open={active}
            autoHideDuration={3000}
            anchorOrigin={{ horizontal: "right", vertical: "bottom" }}
            onClose={() => {
                setActive(false)
            }}
        >
            <Alert
                onClose={() => {
                    setActive(false)
                }}
                severity={severity}
                variant="filled"
            >
                {text}
            </Alert>
        </Snackbar>
    )
}

export default GlobalAlert
