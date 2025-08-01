import type { AlertColor } from "@mui/material"

export let Global_SetAlert: (type: AlertColor, text: string) => void = () => {
    console.warn("unoverriden alert called")
}

export function setGlobalSetAlert(func: typeof Global_SetAlert) {
    Global_SetAlert = func
}
